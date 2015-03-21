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

        public object Any(SyncServiceStackCustomers request)
        {
            var users = DiscourseClient.AdminGetUsers();
            foreach (var discourseUser in users)
            {
                //Don't process discourse administrators
                if (discourseUser.Admin)
                {
                    continue;
                }

                UserServiceResponse existingCustomerSubscription;
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
                        break;
                    }
                }

                try
                {
                    Thread.Sleep(2000);
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
            return null;
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