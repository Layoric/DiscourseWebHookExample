using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using DiscourseAPIClient;
using ServiceStack;
using DiscourseHookTest.ServiceModel;
using DiscourseHookTest.ServiceModel.Types;
using ServiceStack.Configuration;
using ServiceStack.Logging;
using ServiceStack.OrmLite;
using ServiceStack.Text;

namespace DiscourseHookTest.ServiceInterface
{
    public class MyServices : Service
    {
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

        public object Post(UserCreatedDiscourseWebHook req)
        {
            ILog log = LogManager.GetLogger(GetType());
            
            var rawString = req.RequestStream.ToUtf8String();
            log.Info("User registered hook fired. \r\n\r\n" + rawString);
            string apiKey = GetApiKeyFromRequest(rawString);
            //Bug with JsonObjectArray? First character of first array element (string) gets dropped
            // eg, ["testValue",{},{}] first element key equals "estValue"
            if (AppSettings.Get("DiscourseApiKey", "") != apiKey)
            {
                return null;
            }
            var discourseUser = GetUser(rawString);
            log.Info("User email: {0}".Fmt(discourseUser.Email));
            var existingCustomerSubscription = ServiceStackAccountClient.GetUserSubscription(discourseUser.Email);
            if (existingCustomerSubscription != null && 
                existingCustomerSubscription.Expiry != null)
            {
                log.Info("User {0} did have a valid subscription. Approving.");
                try
                {
                    ApproveUser(discourseUser);
                }
                catch (Exception e)
                {
                    log.Error("Error approving user {0} \r\n\r\n {1}".Fmt(discourseUser.Email,e.Message));
                }
                
            }
            else
            {
                log.Info("User {0} did not have a valid subscription");
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
            var jsonArrayObjects = JsonArrayObjects.Parse(rawRequest);
            //First array param is the apiKey from Discourse
            var apiKey = jsonArrayObjects[0].Keys.First();
            return apiKey;
        }
    }

    public interface IServiceStackAccountClient
    {
        UserServiceResponse GetUserSubscription(string emailAddress);
    }

    public class ServiceStackAccountClient : IServiceStackAccountClient
    {
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
                result = JsonSerializer.DeserializeFromString<UserServiceResponse>(
                                serviceUrl.Fmt(emailAddress).GetJsonFromUrl());
            }
            catch (Exception e)
            {
                ILog log = LogManager.GetLogger(GetType());
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