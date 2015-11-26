using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DiscourseAPIClient.Types;
using ServiceStack;
using ServiceStack.Text;

namespace DiscourseAPIClient
{

    public interface IDiscourseClient
    {
        void Login(string userName, string pass);
        GetCategoriesResponse GetCategories();
        GetCategoryResponse GetCategory(int id);
        CreateCategoryResponse CreateCategory(string name, string color, string textColor);
        AdminApproveUserResponse AdminApproveUser(int userId);
        AdminSuspendUserResponse AdminSuspendUser(int userId, int days, string reasonGiven);
        AdminUnsuspendUserResponse AdminUnsuspendUser(int userId);
        ReplyToPostResponse CreateReply(int category, int topicId, int? postId, string content);
        CreatePostResponse CreateTopic(int categoryId, string title, string content);
        GetTopicResponse GetTopic(int id);
        GetLatestTopicsResponse GetTopics();
        List<DiscourseUser> AdminGetUsers(int limit = 100);
        GetUserByIdResponse GetUserById(string userId);
        GetUserEmailByIdResponse GetUserEmail(string userId);
        List<DiscourseUser> AdminFindUsersByFilter(string filter);
    }

    public class DiscourseClient : IDiscourseClient
    {
        public string ApiKey { get; private set; }
        public string UserName { get; private set; }

        private string csrf { get; set; }

        private readonly JsonServiceClient client;

        public DiscourseClient(string url, string apiKey, string userName)
        {
            ApiKey = apiKey;
            UserName = userName;
            client = new JsonServiceClient(url);
            client.Get(url.AppendPath("top.json").AddQueryParam("api_key", apiKey).AddQueryParam("api_username", userName));
        }

        [Route("/session")]
        public class LoginAuth
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        [Route("/session/csrf")]
        public class GetCsrfToken
        {

        }

        public class GetCsrfTokenResponse
        {
            public string csrf { get; set; }
        }

        /// <summary>
        /// All the admin related tasks are not available via just an API key and user, this mimicks logging in a normal user
        /// If the user is admin, admin methods are available.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pass"></param>
        public void Login(string userName, string pass)
        {
            //client.Post <LoginAuthResponse>("/session", new LoginAuth { username = userName, password = pass });
            var csrfWebresponse = client.Get<GetCsrfTokenResponse>(new GetCsrfToken());
            client.Headers.Add("X-CSRF-Token", csrfWebresponse.csrf);
            csrf = csrfWebresponse.csrf;
            client.Headers.Add("X-Request-With", "XMLHttpRequest");
            client.SetCredentials(userName, pass);
            client.GetUrl("/session").PostToUrl(new LoginAuth { username = userName, password = pass }, "*/*",
                webReq =>
                {
                    webReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    webReq.Headers.Add("X-CSRF-Token", csrfWebresponse.csrf);
                    webReq.Headers.Add("X-Request-With", "XMLHttpRequest");
                    webReq.CookieContainer = client.CookieContainer;
                },
                webRes =>
                {
                    client.CookieContainer.Add(webRes.Cookies);
                });
        }

        public GetCategoriesResponse GetCategories()
        {
            using (JsConfig
                .With(propertyConvention: PropertyConvention.Lenient,
                    emitLowercaseUnderscoreNames: true,
                    emitCamelCaseNames: false))
            {
                var request = new GetCategories();
                return client.Get(request);
            }
        }

        public GetCategoryResponse GetCategory(int id)
        {
            using (JsConfig
                .With(propertyConvention: PropertyConvention.Lenient,
                    emitLowercaseUnderscoreNames: true,
                    emitCamelCaseNames: false))
            {
                var request = new GetCategory { Id = id };
                return client.Get(request);
            }
        }

        public GetLatestTopicsResponse GetTopics()
        {
            using (JsConfig
                .With(propertyConvention: PropertyConvention.Lenient,
                    emitLowercaseUnderscoreNames: true,
                    emitCamelCaseNames: false))
            {
                var request = new GetLatestTopics();
                return client.Get(request);
            }
        }

        public List<DiscourseUser> AdminGetUsers(int limit = 100)
        {
            using (JsConfig
                .With(propertyConvention: PropertyConvention.Lenient,
                    emitLowercaseUnderscoreNames: true,
                    emitCamelCaseNames: false))
            {
                var request = new AdminGetUsersWithEmail { limit = limit };
                var requestUrl = request.ToGetUrl()
                    .AddQueryParam("api_key", ApiKey).AddQueryParam("api_username", UserName);
                requestUrl = client.BaseUri.Substring(0, client.BaseUri.Length - 1) + requestUrl;
                var res = requestUrl.GetJsonFromUrl(webReq =>
                {
                    webReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    webReq.Headers.Add("X-CSRF-Token", csrf);
                    webReq.Headers.Add("X-Request-With", "XMLHttpRequest");
                    webReq.CookieContainer = client.CookieContainer;
                }, webRes =>
                {
                    client.CookieContainer.Add(webRes.Cookies);
                });
                return JsonSerializer.DeserializeFromString<List<DiscourseUser>>(res);
            }
        }

        public GetUserByIdResponse GetUserById(string userId)
        {
            using (JsConfig
                .With(propertyConvention: PropertyConvention.Lenient,
                    emitLowercaseUnderscoreNames: true,
                    emitCamelCaseNames: false))
            {
                var request = new GetUserById { UserId = userId};
                var requestUrl = request.ToGetUrl()
                    .AddQueryParam("api_key", ApiKey).AddQueryParam("api_username", UserName);
                requestUrl = client.BaseUri.Substring(0, client.BaseUri.Length - 1) + requestUrl;
                var res = requestUrl.GetJsonFromUrl(webReq =>
                {
                    webReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    webReq.Headers.Add("X-CSRF-Token", csrf);
                    webReq.Headers.Add("X-Request-With", "XMLHttpRequest");
                    webReq.CookieContainer = client.CookieContainer;
                }, webRes =>
                {
                    client.CookieContainer.Add(webRes.Cookies);
                });
                return JsonSerializer.DeserializeFromString<GetUserByIdResponse>(res);
            }
        }

        public GetUserEmailByIdResponse GetUserEmail(string userId)
        {
            using (JsConfig
                .With(propertyConvention: PropertyConvention.Lenient,
                    emitLowercaseUnderscoreNames: true,
                    emitCamelCaseNames: false))
            {
                var request = new AdminGetUserEmailById { UserId = userId };
                var requestUrl = request.ToGetUrl()
                    .AddQueryParam("api_key", ApiKey).AddQueryParam("api_username", UserName);
                requestUrl = client.BaseUri.Substring(0, client.BaseUri.Length - 1) + requestUrl;
                var res = requestUrl.GetJsonFromUrl(webReq =>
                {
                    webReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    webReq.Headers.Add("X-CSRF-Token", csrf);
                    webReq.Headers.Add("X-Request-With", "XMLHttpRequest");
                    webReq.CookieContainer = client.CookieContainer;
                }, webRes =>
                {
                    client.CookieContainer.Add(webRes.Cookies);
                });
                return JsonSerializer.DeserializeFromString<GetUserEmailByIdResponse>(res);
            }
        }

        public List<DiscourseUser> AdminFindUsersByFilter(string filter)
        {
            using (JsConfig
               .With(propertyConvention: PropertyConvention.Lenient,
                   emitLowercaseUnderscoreNames: true,
                   emitCamelCaseNames: false))
            {
                var request = new AdminGetUsers();
                var requestUrl = request.ToGetUrl()
                    .AddQueryParam("api_key", ApiKey).AddQueryParam("api_username", UserName)
                    .AddQueryParam("filter", filter);
                requestUrl = client.BaseUri.Substring(0, client.BaseUri.Length - 1) + requestUrl;
                var res = requestUrl.GetJsonFromUrl(webReq =>
                {
                    webReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    webReq.Headers.Add("X-CSRF-Token", csrf);
                    webReq.Headers.Add("X-Request-With", "XMLHttpRequest");
                    webReq.CookieContainer = client.CookieContainer;
                }, webRes =>
                {
                    client.CookieContainer.Add(webRes.Cookies);
                });
                return JsonSerializer.DeserializeFromString<List<DiscourseUser>>(res);
            }
        }

        public GetUserEmailByIdResponse AdminGetUserEmailById(string userId)
        {
            using (JsConfig
                .With(propertyConvention: PropertyConvention.Lenient,
                    emitLowercaseUnderscoreNames: true,
                    emitCamelCaseNames: false))
            {
                var request = new AdminGetUserEmailById { UserId = userId };
                var requestUrl = request.ToGetUrl()
                    .AddQueryParam("api_key", ApiKey).AddQueryParam("api_username", UserName);
                requestUrl = client.BaseUri.Substring(0, client.BaseUri.Length - 1) + requestUrl;
                var res = requestUrl.GetJsonFromUrl(webReq =>
                {
                    webReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    webReq.Headers.Add("X-CSRF-Token", csrf);
                    webReq.Headers.Add("X-Request-With", "XMLHttpRequest");
                    webReq.CookieContainer = client.CookieContainer;
                }, webRes =>
                {
                    client.CookieContainer.Add(webRes.Cookies);
                });
                return JsonSerializer.DeserializeFromString<GetUserEmailByIdResponse>(res);
            }
        }

        public GetTopicResponse GetTopic(int id)
        {
            using (JsConfig
                .With(propertyConvention: PropertyConvention.Lenient,
                    emitLowercaseUnderscoreNames: true,
                    emitCamelCaseNames: false))
            {
                var request = new GetTopic { TopicId = id };
                return client.Get(request);
            }
        }

        public CreatePostResponse CreateTopic(int categoryId, string title, string content)
        {
            using (JsConfig
                .With(propertyConvention: PropertyConvention.Lenient,
                    emitLowercaseUnderscoreNames: true,
                    emitCamelCaseNames: false))
            {
                var request = new CreateTopic { Category = categoryId, Title = title, Raw = content };
                var requestUrl = request.ToPostUrl()
                    .AddQueryParam("api_key", ApiKey).AddQueryParam("api_username", UserName);
                requestUrl = client.BaseUri.Substring(0, client.BaseUri.Length - 1) + requestUrl;

                return client.Post<CreatePostResponse>(requestUrl, request);
            }
        }

        public ReplyToPostResponse CreateReply(int category, int topicId, int? postId, string content)
        {
            using (JsConfig
                .With(propertyConvention: PropertyConvention.Lenient,
                    emitLowercaseUnderscoreNames: true,
                    emitCamelCaseNames: false))
            {
                var request = new ReplyToPost
                {
                    Category = category,
                    TopicId = topicId,
                    ReplyToPostNumber = postId,
                    Raw = content
                };
                var requestUrl = request.ToPostUrl()
                    .AddQueryParam("api_key", ApiKey).AddQueryParam("api_username", UserName);
                requestUrl = client.BaseUri.Substring(0, client.BaseUri.Length - 1) + requestUrl;

                return client.Post<ReplyToPostResponse>(requestUrl, request);
            }
        }

        /// <summary>
        /// Requires Login method to be used as not an normal API call
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public AdminApproveUserResponse AdminApproveUser(int userId)
        {
            using (JsConfig
                .With(propertyConvention: PropertyConvention.Lenient,
                    emitLowercaseUnderscoreNames: true,
                    emitCamelCaseNames: false))
            {
                var request = new AdminApproveUser { UserId = userId };
                var requestUrl = request.ToPutUrl()
                    .AddQueryParam("api_key", ApiKey).AddQueryParam("api_username", UserName);
                requestUrl = client.BaseUri.Substring(0, client.BaseUri.Length - 1) + requestUrl;
                var res = requestUrl.PutToUrl(null, "*/*", webReq =>
                {
                    webReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    webReq.Headers.Add("X-CSRF-Token", csrf);
                    webReq.Headers.Add("X-Request-With", "XMLHttpRequest");
                    webReq.CookieContainer = client.CookieContainer;
                }, webRes =>
                {
                    client.CookieContainer.Add(webRes.Cookies);
                });
                return JsonSerializer.DeserializeFromString<AdminApproveUserResponse>(res);
            }
        }

        /// <summary>
        /// Requires Login method to be used as not an normal API call
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="days"></param>
        /// <param name="reasonGiven"></param>
        /// <returns></returns>
        public AdminSuspendUserResponse AdminSuspendUser(int userId, int days, string reasonGiven)
        {
            using (JsConfig
                .With(propertyConvention: PropertyConvention.Lenient,
                    emitLowercaseUnderscoreNames: true,
                    emitCamelCaseNames: false))
            {
                var request = new AdminSuspendUser { UserId = userId };
                var requestUrl = request.ToPutUrl()
                    .AddQueryParam("api_key", ApiKey).AddQueryParam("api_username", UserName);
                requestUrl = client.BaseUri.Substring(0, client.BaseUri.Length - 1) + requestUrl;
                var res = requestUrl.PutToUrl(new { duration = days, reason = reasonGiven }, "*/*", webReq =>
                {
                    webReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    webReq.Headers.Add("X-CSRF-Token", csrf);
                    webReq.Headers.Add("X-Request-With", "XMLHttpRequest");
                    webReq.CookieContainer = client.CookieContainer;
                }, webRes =>
                {
                    client.CookieContainer.Add(webRes.Cookies);
                });
                return JsonSerializer.DeserializeFromString<AdminSuspendUserResponse>(res);
            }
        }

        /// <summary>
        /// Requires Login method to be used as not an normal API call
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public AdminUnsuspendUserResponse AdminUnsuspendUser(int userId)
        {
            using (JsConfig
                .With(propertyConvention: PropertyConvention.Lenient,
                    emitLowercaseUnderscoreNames: true,
                    emitCamelCaseNames: false))
            {
                var request = new AdminUnsuspendUser { UserId = userId };
                var requestUrl = request.ToPutUrl()
                    .AddQueryParam("api_key", ApiKey).AddQueryParam("api_username", UserName);
                requestUrl = client.BaseUri.Substring(0, client.BaseUri.Length - 1) + requestUrl;
                var res = requestUrl.PutToUrl(null, "*/*", webReq =>
                {
                    webReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    webReq.Headers.Add("X-CSRF-Token", csrf);
                    webReq.Headers.Add("X-Request-With", "XMLHttpRequest");
                    webReq.CookieContainer = client.CookieContainer;
                }, webRes =>
                {
                    client.CookieContainer.Add(webRes.Cookies);
                });
                return JsonSerializer.DeserializeFromString<AdminUnsuspendUserResponse>(res);
            }
        }

        public CreateCategoryResponse CreateCategory(string name, string color, string textColor)
        {
            using (JsConfig
                .With(propertyConvention: PropertyConvention.Lenient,
                    emitLowercaseUnderscoreNames: true,
                    emitCamelCaseNames: false))
            {
                var request = new CreateCategory { Name = name, Color = color, TextColor = textColor, CategorySlug = name.GenerateSlug() };
                var requestUrl = request.ToPostUrl()
                    .AddQueryParam("api_key", ApiKey).AddQueryParam("api_username", UserName);
                requestUrl = client.BaseUri.Substring(0, client.BaseUri.Length - 1) + requestUrl;

                return client.Post<CreateCategoryResponse>(requestUrl, request);
            }
        }
    }

    public class GetUserByIdResponse
    {
        public DiscourseUser User { get; set; }
    }


    [Route("/users/{Username}", "PUT")]
    public class AdminUpdateUser
    {
        public string Username { get; set; }
        public bool Blocked { get; set; }
    }

    [Route("/categories.json", "GET")]
    public class GetCategories : IReturn<GetCategoriesResponse>
    {

    }

    [Route("/c/{Id}", "GET")]
    public class GetCategory : IReturn<GetCategoryResponse>
    {
        public int Id { get; set; }
    }

    [Route("/latest.json", "GET")]
    public class GetLatestTopics : IReturn<GetLatestTopicsResponse>
    {

    }

    [Route("/t/{TopicId}", "GET")]
    public class GetTopic : IReturn<GetTopicResponse>
    {
        public int TopicId { get; set; }
    }

    [Route("/admin/users.json?show_emails=true", "GET")]
    public class AdminGetUsersWithEmail : IReturn<List<DiscourseUser>>
    {
        public int limit { get; set; }
    }

    [Route("/admin/users")]
    public class AdminGetUsers : IReturn<List<DiscourseUser>>
    {
    }

    [Route("/users/{UserId}/emails.json")]
    public class AdminGetUserEmailById : IReturn<GetUserEmailByIdResponse>
    {
        public string UserId { get; set; }
    }

    public class GetUserEmailByIdResponse
    {
        public string Email { get; set; }
        public string AssociatedAccounts { get; set; }
    }

    [Route("/users/{UserId}")]
    public class GetUserById : IReturn<DiscourseUser>
    {
        public string UserId { get; set; }
    }

    [Route("/posts", "POST")]
    public class CreateTopic : IReturn<CreatePostResponse>
    {
        public int Category { get; set; }
        public string Title { get; set; }
        public string Raw { get; set; }
    }

    [Route("/categories", "POST")]
    public class CreateCategory : IReturn<CreateCategoryResponse>
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public string TextColor { get; set; }
        public string CategorySlug { get; set; }
    }

    [Route("/admin/users/{UserId}/approve", "PUT")]
    public class AdminApproveUser : IReturn<AdminApproveUserResponse>
    {
        public int UserId { get; set; }
    }

    [Route("/admin/users/{UserId}/suspend", "PUT")]
    public class AdminSuspendUser : IReturn<AdminSuspendUserResponse>
    {
        public int UserId { get; set; }
    }

    [Route("/admin/users/{UserId}/unsuspend", "PUT")]
    public class AdminUnsuspendUser : IReturn<AdminUnsuspendUserResponse>
    {
        public int UserId { get; set; }
    }

    [Route("/posts", "POST")]
    public class ReplyToPost : IReturn<ReplyToPostResponse>
    {
        public int Category { get; set; }
        public int TopicId { get; set; }
        public int? ReplyToPostNumber { get; set; }
        public string Raw { get; set; }
    }

    public static class Extensions
    {
        /// <summary>
        /// From http://stackoverflow.com/a/2921135/670151
        /// </summary>
        /// <param name="phrase"></param>
        /// <returns></returns>
        public static string GenerateSlug(this string phrase)
        {
            string str = phrase.RemoveAccent().ToLower()
                .Replace("#", "sharp")  // c#, f# => csharp, fsharp
                .Replace("+", "p");      // c++ => cpp

            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "-");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            return str;
        }

        public static string RemoveAccent(this string txt)
        {
            byte[] bytes = Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return Encoding.ASCII.GetString(bytes);
        }
    }
}
