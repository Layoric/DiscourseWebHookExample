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
    public class DailyServices : Service
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DailyServices));

        public IAppSettings AppSettings { get; set; }
        public IDiscourseClient DiscourseClient { get; set; }
        public IServiceStackAccountClient ServiceStackAccountClient { get; set; }

        public object Any(SyncAccountsDaily request)
        {
            var users = DiscourseClient.AdminGetUsers(1000);
            foreach (var discourseUser in users)
            {
                //Don't process discourse administrators or already suspended users
                if (discourseUser.Admin || discourseUser.Suspended == true)
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

                //Skip users with valid SS subscriptions.
                if (existingCustomerSubscription.HasValidSubscription())
                {
                    continue;
                }

                try
                {
                    Thread.Sleep(2000);
                    //Existing user with active account but without a valid subscription
                    if (!discourseUser.NeedsApproval() && discourseUser.Suspended != true)
                    {
                        Log.Info("Suspending user '{0}'.".Fmt(discourseUser.Email));
                        SuspendUser(discourseUser);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Failed to suspend Discourse for user '{0}'. - {1}".Fmt(discourseUser.Email, e.Message));
                }
            }
            return null;
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
    }
}
