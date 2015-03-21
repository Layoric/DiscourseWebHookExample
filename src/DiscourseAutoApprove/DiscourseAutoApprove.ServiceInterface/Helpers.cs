using System;
using DiscourseAPIClient.Types;
using ServiceStack;
using ServiceStack.Text;

namespace DiscourseAutoApprove.ServiceInterface
{
    public static class Helpers
    {
        public static DiscourseUser GetUserFromRequest(string rawRequest)
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

        public static string GetApiKeyFromRequest(string rawRequest)
        {
            var parts = rawRequest.SplitOnFirst(',');
            var apiKey = parts[0].Trim('[', '"');
            return apiKey;
        }

        public static bool NeedsApproval(this DiscourseUser user)
        {
            return !user.Approved;
        }

        public static bool HasValidSubscription(this UserServiceResponse serviceStackAccount)
        {
            return
                serviceStackAccount != null &&
                serviceStackAccount.Expiry != null &&
                serviceStackAccount.Expiry > DateTime.Now;
        }
    }
}
