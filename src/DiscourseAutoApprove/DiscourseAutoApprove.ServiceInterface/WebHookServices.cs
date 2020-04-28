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
    public class WebHookServices : Service
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WebHookServices));

        public IAppSettings AppSettings { get; set; }
        public IDiscourseClient DiscourseClient { get; set; }
        public IServiceStackAccountClient ServiceStackAccountClient { get; set; }

        public object Post(UserCreatedDiscourseWebHook request)
        {
            var rawString = request.RequestStream.ToUtf8String();
            Log.Info("User registered hook fired. \r\n\r\n" + rawString);
            string apiKey = Helpers.GetApiKeyFromRequest(rawString);
            if (AppSettings.Get("DiscourseApiKey", "") != apiKey)
            {
                Log.Warn("Invalid api key used - {0}.".Fmt(apiKey));
            }
            var discourseUser = Helpers.GetUserFromRequest(rawString);
            Log.Info("User email: {0}".Fmt(discourseUser.Email));
            var existingCustomerSubscription = ServiceStackAccountClient.GetUserSubscription(discourseUser.Email);
            if (existingCustomerSubscription != null &&
                existingCustomerSubscription.Expiry != null &&
                existingCustomerSubscription.Expiry > DateTime.Now)
            {
                Log.Info("User {0} with email {1} did have a valid subscription. Approving.".Fmt(discourseUser.Id, discourseUser.Email));
                ThreadPool.QueueUserWorkItem(BackgroundApprove, discourseUser);
                //ApproveUser(discourseUser);
            }
            else
            {
                Log.Info("User {0} with email {1} did not have a valid subscription".Fmt(discourseUser.Id, discourseUser.Email));
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
                Log.Error(discourseUser != null
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
    }
}
