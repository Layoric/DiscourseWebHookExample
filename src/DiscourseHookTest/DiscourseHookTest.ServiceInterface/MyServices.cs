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
using ServiceStack.OrmLite;
using ServiceStack.Text;

namespace DiscourseHookTest.ServiceInterface
{
    public class MyServices : Service
    {
        public IAppSettings AppSettings { get; set; }

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
            var rawString = req.RequestStream.ToUtf8String();
            string apiKey = GetApiKeyFromRequest(rawString);
            //Bug with JsonObjectArray? First character of first array element (string) gets dropped
            if (AppSettings.Get("DiscourseApiKey", "") != apiKey)
            {
                return null;
            }
            var discourseUser = GetUser(rawString);

            var existingCustomer = Db.Single<ServiceStackCustomer>(x => x.Email == discourseUser.Email);
            if (existingCustomer != null)
            {
                ApproveUser(discourseUser);
            }
            
            return null;
        }

        private void ApproveUser(DiscourseUser discourseUser)
        {
            var client = new DiscourseClient(
                AppSettings.Get("DiscourseRemoteUrl", ""),
                AppSettings.Get("DiscourseAdminApiKey", ""),
                AppSettings.Get("DiscourseAdminUserName", ""));
            client.Login(AppSettings.Get("DiscourseAdminUserName", ""), AppSettings.Get("DiscourseAdminPassword", ""));
            client.AdminApproveUser(discourseUser.Id);
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

    [FallbackRoute("/{PathInfo*}")]
    public class FallbackForClientRoutes
    {
        public string PathInfo { get; set; }
    }
}