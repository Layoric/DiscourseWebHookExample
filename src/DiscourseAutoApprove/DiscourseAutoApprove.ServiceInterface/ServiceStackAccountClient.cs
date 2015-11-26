using System;
using ServiceStack;
using ServiceStack.Logging;

namespace DiscourseAutoApprove.ServiceInterface
{
    public interface IServiceStackAccountClient
    {
        UserServiceResponse GetUserSubscription(string emailAddress);
    }

    public class ServiceStackAccountClient : IServiceStackAccountClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceStackAccountClient));

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
                Log.Error(e.Message);
                throw;
            }

            return result;
        }
    }

    public class UserServiceResponse
    {
        public DateTime? Expiry { get; set; }
    }
}
