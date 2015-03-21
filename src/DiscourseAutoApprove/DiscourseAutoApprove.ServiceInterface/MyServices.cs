using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using DiscourseAPIClient;
using DiscourseAPIClient.Types;
using DiscourseAutoApprove.ServiceModel;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Logging;
using ServiceStack.Text;

namespace DiscourseAutoApprove.ServiceInterface
{
    public class MyServices : Service
    {
        private static ILog log = LogManager.GetLogger(typeof(MyServices));

        public IAppSettings AppSettings { get; set; }
        public IDiscourseClient DiscourseClient { get; set; }
        public IServiceStackAccountClient ServiceStackAccountClient { get; set; }

        public object Any(Hello request)
        {
            return new HelloResponse { Result = "Hello, {0}!".Fmt(request.Name) };
        }

        public object Any(FallbackForClientRoutes request)
        {
            //Return default.cshtml for unmatched requests so routing is handled on the client
            return new HttpResult
            {
                View = "/default.cshtml"
            };
        }

        public object Any(SyncServiceStackCustomers request)
        {
            var users = DiscourseClient.AdminGetUsers();
            foreach (var user in users)
            {
                //Don't process discourse administrators
                if (user.Admin)
                {
                    continue;
                }

                UserServiceResponse existingCustomerSubscription;
                try
                {
                    existingCustomerSubscription = ServiceStackAccountClient.GetUserSubscription(user.Email);
                }
                catch (Exception e)
                {
                    log.Error("Failed to check user's subscription. Retrying... - {0}".Fmt(e.Message));
                    try
                    {
                        existingCustomerSubscription = ServiceStackAccountClient.GetUserSubscription(user.Email);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Failed to check user's subscription. Cancelling sync. - {0}".Fmt(ex.Message));
                        break;
                    }
                }

                try
                {
                    Thread.Sleep(2000);
                    if (UserNeedsApproval(user) && UserHasValidSubscription(existingCustomerSubscription))
                    {
                        log.Info("Approving user '{0}'.".Fmt(user.Email));
                        ApproveUser(user);
                    }

                    if (!UserNeedsApproval(user) && !UserHasValidSubscription(existingCustomerSubscription))
                    {
                        log.Info("Suspending user '{0}'.".Fmt(user.Email));
                        SuspendUser(user);
                    }

                    if (!UserNeedsApproval(user) && user.Suspended == true)
                    {
                        log.Info("Unsuspending user '{0}'.".Fmt(user.Email));
                        UnsuspendUser(user);
                    }
                }
                catch (Exception e)
                {
                    log.Error("Failed to update Discourse for user '{0}'. - {1}".Fmt(user.Email, e.Message));
                }
            }
            return null;
        }

        private bool UserNeedsApproval(DiscourseUser user)
        {
            return !user.Approved;
        }

        private bool UserHasValidSubscription(UserServiceResponse serviceStackAccount)
        {
            return
                serviceStackAccount != null &&
                serviceStackAccount.Expiry != null &&
                serviceStackAccount.Expiry > DateTime.Now;
        }

        public object Post(UserCreatedDiscourseWebHook request)
        {
            var rawString = request.RequestStream.ToUtf8String();
            log.Info("User registered hook fired. \r\n\r\n" + rawString);
            string apiKey = GetApiKeyFromRequest(rawString);
            if (AppSettings.Get("DiscourseApiKey", "") != apiKey)
            {
                log.Warn("Invalid api key used - {0}.".Fmt(apiKey));
            }
            var discourseUser = GetUser(rawString);
            log.Info("User email: {0}".Fmt(discourseUser.Email));
            var existingCustomerSubscription = ServiceStackAccountClient.GetUserSubscription(discourseUser.Email);
            if (existingCustomerSubscription != null &&
                existingCustomerSubscription.Expiry != null &&
                existingCustomerSubscription.Expiry > DateTime.Now)
            {
                log.Info("User {0} with email {1} did have a valid subscription. Approving.".Fmt(discourseUser.Id, discourseUser.Email));
                ThreadPool.QueueUserWorkItem(BackgroundApprove, discourseUser);
            }
            else
            {
                log.Info("User {0} with email {1} did not have a valid subscription".Fmt(discourseUser.Id, discourseUser.Email));
            }
            return null;
        }

        private void BackgroundApprove(object user)
        {
            Thread.Sleep(3000);
            var discourseUser = user as DiscourseUser;

            try
            {
                ApproveUser(discourseUser);
            }
            catch (Exception e)
            {
                log.Error(discourseUser != null
                    ? "Error approving user {0} \r\n\r\n {1}".Fmt(discourseUser.Email, e.Message)
                    : "Error approving user {0} \r\n\r\n Discourse user null");
            }
        }

        private void ApproveUser(DiscourseUser discourseUser)
        {
            try
            {
                DiscourseClient.AdminApproveUser(discourseUser.Id);
            }
            catch (Exception)
            {
                //Try to login again and retry
                DiscourseClient.Login(AppSettings.Get("DiscourseAdminUserName", ""), AppSettings.Get("DiscourseAdminPassword", ""));
                DiscourseClient.AdminApproveUser(discourseUser.Id);
            }
        }

        private void SuspendUser(DiscourseUser user)
        {
            try
            {
                DiscourseClient.AdminSuspendUser(user.Id, 365, AppSettings.GetString("DiscourseSuspensionReason"));
            }
            catch (Exception)
            {
                //Try to login again and retry
                DiscourseClient.Login(AppSettings.Get("DiscourseAdminUserName", ""), AppSettings.Get("DiscourseAdminPassword", ""));
                DiscourseClient.AdminSuspendUser(user.Id, 365, AppSettings.GetString("DiscourseSuspensionReason"));
            }
        }

        private void UnsuspendUser(DiscourseUser user)
        {
            try
            {
                DiscourseClient.AdminUnsuspendUser(user.Id);
            }
            catch (Exception)
            {
                //Try to login again and retry
                DiscourseClient.Login(AppSettings.Get("DiscourseAdminUserName", ""), AppSettings.Get("DiscourseAdminPassword", ""));
                DiscourseClient.AdminUnsuspendUser(user.Id);
            }
        }

        private DiscourseUser GetUser(string rawRequest)
        {
            var jsonArrayObjects = JsonArrayObjects.Parse(rawRequest);
            DiscourseUser discourseUser;

            //Second object is all other params, of which there are "user" and "email"
            using (JsConfig
                .With(propertyConvention: PropertyConvention.Lenient,
                    emitLowercaseUnderscoreNames: true,
                    emitCamelCaseNames: false))
            {
                discourseUser = JsonSerializer.DeserializeFromString<DiscourseUser>(jsonArrayObjects[1].Child("user"));
            }

            string email = jsonArrayObjects[1].Child("email");
            discourseUser.Email = email;
            return discourseUser;
        }

        private string GetApiKeyFromRequest(string rawRequest)
        {
            var parts = rawRequest.SplitOnFirst(',');
            var apiKey = parts[0].Trim('[', '"');
            return apiKey;
        }
    }

    public interface IServiceStackAccountClient
    {
        UserServiceResponse GetUserSubscription(string emailAddress);
    }

    public class ServiceStackAccountClient : IServiceStackAccountClient
    {
        private static ILog log = LogManager.GetLogger(typeof(ServiceStackAccountClient));

        private readonly string serviceUrl;
        public ServiceStackAccountClient(string url)
        {
            serviceUrl = url;
        }

        public UserServiceResponse GetUserSubscription(string emailAddress)
        {
            UserServiceResponse result = null;
            try
            {
                result = serviceUrl.Fmt(emailAddress).GetJsonFromUrl()
                    .FromJson<UserServiceResponse>();
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }

            return result;
        }
    }

    public class UserServiceResponse
    {
        public DateTime? Expiry { get; set; }
    }

    [FallbackRoute("/{PathInfo*}")]
    public class FallbackForClientRoutes
    {
        public string PathInfo { get; set; }
    }
}