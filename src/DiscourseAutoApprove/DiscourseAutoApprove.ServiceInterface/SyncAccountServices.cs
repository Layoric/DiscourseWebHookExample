using System;
using System.Threading;
using DiscourseAPIClient;
using DiscourseAPIClient.Types;
using DiscourseAutoApprove.ServiceModel;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Logging;

namespace DiscourseAutoApprove.ServiceInterface
{
    public class SyncAccountServices : Service
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SyncAccountServices));

        public IAppSettings AppSettings { get; set; }
        public IDiscourseClient DiscourseClient { get; set; }
        public IServiceStackAccountClient ServiceStackAccountClient { get; set; }

        public void Any(SyncServiceStackCustomers request)
        {
            var users = DiscourseClient.AdminGetUsers(1000);
            foreach (var discourseUser in users)
            {
                //Don't process discourse administrators
                if (discourseUser.Admin)
                {
                    continue;
                }

                var existingCustomerSubscription = GetDiscourseUserServiceStackSubscription(discourseUser);

                UpdateDiscourseAccountStatus(discourseUser, existingCustomerSubscription);
            }
        }

        private void UpdateDiscourseAccountStatus(DiscourseUser discourseUser, UserServiceResponse existingCustomerSubscription, int throttleRequestTime = 2000)
        {
            try
            {
                Thread.Sleep(throttleRequestTime);
                if (discourseUser.NeedsApproval() && existingCustomerSubscription.HasValidSubscription())
                {
                    Log.Info("Approving user '{0}'.".Fmt(discourseUser.Email));
                    ApproveUser(discourseUser);
                }
            }
            catch (Exception e)
            {
                Log.Error("Failed to update Discourse for user '{0}'. - {1}".Fmt(discourseUser.Email, e.Message));
            }
        }

        private UserServiceResponse GetDiscourseUserServiceStackSubscription(DiscourseUser discourseUser)
        {
            UserServiceResponse existingCustomerSubscription = null;
            try
            {
                existingCustomerSubscription = ServiceStackAccountClient.GetUserSubscription(discourseUser.Email);
            }
            catch (Exception e)
            {
                Log.Error("Failed to check user's subscription. Retrying... - {0}".Fmt(e.Message));
                try
                {
                    existingCustomerSubscription = ServiceStackAccountClient.GetUserSubscription(discourseUser.Email);
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to check user's subscription. Cancelling sync. - {0}".Fmt(ex.Message));
                    return existingCustomerSubscription;
                }
            }
            return existingCustomerSubscription;
        }

        public void Any(SyncSingleUser request)
        {
            if (request.UserId == null)
            {
                throw new HttpError(400,"BadRequest");
            }

            GetUserByIdResponse user;
            try
            {
                user = DiscourseClient.GetUserById(request.UserId);
            }
            catch (Exception e)
            {
                Log.Error("Failed to get user from Discourse - {0}".Fmt(e.Message), e);
                throw new HttpError(500,"Failed to get user from Discourse");
            }

            if (user == null)
            {
                throw HttpError.NotFound("User not found in Discourse"); 
            }
            // Email is ONLY populated when performing admin get all users as Discourse API allows it via show_emails
            // query string. Single user request doesn't have same rules..
            var emailResponse = DiscourseClient.GetUserEmail(request.UserId);
            user.User.Email = emailResponse.Email;

            var serviceStackSubscription = GetDiscourseUserServiceStackSubscription(user.User);
            UpdateDiscourseAccountStatus(user.User,serviceStackSubscription,1000);
        }

        public void Any(SyncListOfUsers request)
        {
            if (request.UserIds == null)
            {
                throw new HttpError(400,"MissingUserIDs");
            }

            foreach (var userId in request.UserIds)
            {
                Log.Info("Updating {0} account status.".Fmt(userId));
                Any(new SyncSingleUser {UserId = userId});
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
    }

    [FallbackRoute("/{PathInfo*}")]
    public class FallbackForClientRoutes
    {
        public string PathInfo { get; set; }
    }
}