using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DiscourseAPIClient;
using DiscourseAPIClient.Types;
using DiscourseAutoApprove.ServiceInterface;
using DiscourseAutoApprove.ServiceModel;
using Funq;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.Support;
using ServiceStack.Testing;

namespace DiscourseAutoApprove.Tests
{
    [TestFixture]
    public class UnitTests
    {
        private readonly ServiceStackHost appHost;

        public UnitTests()
        {
            appHost = new BasicAppHost(typeof(MyServices).Assembly)
            {
                ConfigureContainer = ConfigureAppHost
            };
            SeedAppSettings(appHost);
            appHost.Init();
        }

        private string invalidEmailInput = "[\"_testapikey\",{\"user\":{\"id\":21,\"username\":\"layoric_u15\",\"uploaded_avatar_id\":null,\"avatar_template\":\"/letter_avatar/layoric_u15/{size}/2.png\",\"name\":\"U15\",\"last_posted_at\":null,\"last_seen_at\":null,\"created_at\":\"2015-03-14T20:59:59.585Z\",\"can_edit\":true,\"can_edit_username\":true,\"can_edit_email\":true,\"can_edit_name\":true,\"stats\":[],\"can_send_private_messages\":false,\"can_send_private_message_to_user\":true,\"bio_excerpt\":\"\u003cdiv class='missing-profile'\u003eU15 hasn't entered anything in the About Me field of their profile yet\u003c/div\u003e\",\"trust_level\":0,\"moderator\":false,\"admin\":false,\"title\":null,\"badge_count\":0,\"notification_count\":0,\"has_title_badges\":false,\"custom_fields\":{},\"post_count\":0,\"can_be_deleted\":true,\"can_delete_all_posts\":true,\"locale\":null,\"email_digests\":true,\"email_private_messages\":true,\"email_direct\":true,\"email_always\":false,\"digest_after_days\":7,\"mailing_list_mode\":false,\"auto_track_topics_after_msecs\":240000,\"new_topic_duration_minutes\":2880,\"external_links_in_new_tab\":false,\"dynamic_favicon\":false,\"enable_quoting\":true,\"muted_category_ids\":[],\"tracked_category_ids\":[],\"watched_category_ids\":[],\"private_messages_stats\":{\"all\":0,\"mine\":0,\"unread\":0},\"disable_jump_reply\":false,\"gravatar_avatar_upload_id\":null,\"custom_avatar_upload_id\":null,\"single_sign_on_record\":null,\"invited_by\":null,\"custom_groups\":[],\"featured_user_badge_ids\":[],\"card_badge\":null},\"email\":\"layoric+u15@gmail.com\"}]";
        private string validEmailInput = "[\"_testapikey\",{\"user\":{\"id\":21,\"username\":\"layoric_success\",\"uploaded_avatar_id\":null,\"avatar_template\":\"/letter_avatar/layoric_u15/{size}/2.png\",\"name\":\"U15\",\"last_posted_at\":null,\"last_seen_at\":null,\"created_at\":\"2015-03-14T20:59:59.585Z\",\"can_edit\":true,\"can_edit_username\":true,\"can_edit_email\":true,\"can_edit_name\":true,\"stats\":[],\"can_send_private_messages\":false,\"can_send_private_message_to_user\":true,\"bio_excerpt\":\"\u003cdiv class='missing-profile'\u003eU15 hasn't entered anything in the About Me field of their profile yet\u003c/div\u003e\",\"trust_level\":0,\"moderator\":false,\"admin\":false,\"title\":null,\"badge_count\":0,\"notification_count\":0,\"has_title_badges\":false,\"custom_fields\":{},\"post_count\":0,\"can_be_deleted\":true,\"can_delete_all_posts\":true,\"locale\":null,\"email_digests\":true,\"email_private_messages\":true,\"email_direct\":true,\"email_always\":false,\"digest_after_days\":7,\"mailing_list_mode\":false,\"auto_track_topics_after_msecs\":240000,\"new_topic_duration_minutes\":2880,\"external_links_in_new_tab\":false,\"dynamic_favicon\":false,\"enable_quoting\":true,\"muted_category_ids\":[],\"tracked_category_ids\":[],\"watched_category_ids\":[],\"private_messages_stats\":{\"all\":0,\"mine\":0,\"unread\":0},\"disable_jump_reply\":false,\"gravatar_avatar_upload_id\":null,\"custom_avatar_upload_id\":null,\"single_sign_on_record\":null,\"invited_by\":null,\"custom_groups\":[],\"featured_user_badge_ids\":[],\"card_badge\":null},\"email\":\"success@gmail.com\"}]";

        public void SeedAppSettings(ServiceStackHost basicAppHost)
        {
            basicAppHost.AppSettings.Set("DiscourseApiKey", "_testapikey");
        }

        public void ConfigureAppHost(Container container)
        {
            LogManager.LogFactory = new InMemoryLogFactory();
            container.Register(appHost.AppSettings);
            container.Register<IDiscourseClient>(new MockDiscourseClient());
            container.Register<IServiceStackAccountClient>(new MockServiceStackAccountClient());
        }

        [SetUp]
        public void Setup()
        {
            var discourseClient = appHost.Container.Resolve<IDiscourseClient>() as MockDiscourseClient;
            discourseClient.ApproveCalledCount = 0;
            discourseClient.SuspendCalledCount = 0;
            discourseClient.UnsuspendCalledCount = 0;
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            appHost.Dispose();
        }

        [Test]
        public void TestMethod1()
        {
            var service = appHost.Container.Resolve<MyServices>();

            var response = (HelloResponse)service.Any(new Hello { Name = "World" });

            Assert.That(response.Result, Is.EqualTo("Hello, World!"));
        }

        [Test]
        public void TestUserWithoutSubscription()
        {
            var service = appHost.Container.Resolve<MyServices>();

            var req = new UserCreatedDiscourseWebHook {RequestStream = new MemoryStream(invalidEmailInput.ToUtf8Bytes())};
            service.Post(req);
            Thread.Sleep(3200);
            var discourseClient = appHost.Resolve<IDiscourseClient>() as MockDiscourseClient;
            Assert.That(discourseClient != null);
            Assert.That(discourseClient.ApproveCalledCount == 0);
        }

        [Test]
        public void TestUserWithSubscription()
        {
            var service = appHost.Container.Resolve<MyServices>();

            var req = new UserCreatedDiscourseWebHook { RequestStream = new MemoryStream(validEmailInput.ToUtf8Bytes()) };
            service.Post(req);
            Thread.Sleep(3200);
            var discourseClient = appHost.Resolve<IDiscourseClient>() as MockDiscourseClient;
            Assert.That(discourseClient != null);
            Assert.That(discourseClient.ApproveCalledCount > 0);
        }

        [Test]
        public void TestSyncAllUsers()
        {
            var service = appHost.Container.Resolve<MyServices>();

            var req = new SyncServiceStackCustomers();
            service.Any(req);
            var discourseClient = appHost.Resolve<IDiscourseClient>() as MockDiscourseClient;
            Assert.That(discourseClient != null);
            Assert.That(discourseClient.ApproveCalledCount == 1);
            Assert.That(discourseClient.SuspendCalledCount == 1);
            Assert.That(discourseClient.UnsuspendCalledCount == 1);
        }
    }

    public class MockServiceStackAccountClient : IServiceStackAccountClient
    {
        public UserServiceResponse GetUserSubscription(string emailAddress)
        {
            //False test case
            if (emailAddress == "layoric+u15@gmail.com")
                return new UserServiceResponse();

            //True test case
            return new UserServiceResponse {Expiry = new DateTime(2016, 1, 1)};
        }
    }

    public class MockDiscourseClient : IDiscourseClient
    {
        public int ApproveCalledCount { get; set; }
        public int SuspendCalledCount { get; set; }
        public int UnsuspendCalledCount { get; set; }
        public void Login(string userName, string pass)
        {
            throw new NotImplementedException();
        }

        public GetCategoriesResponse GetCategories()
        {
            throw new NotImplementedException();
        }

        public GetCategoryResponse GetCategory(int id)
        {
            throw new NotImplementedException();
        }

        public CreateCategoryResponse CreateCategory(string name, string color, string textColor)
        {
            throw new NotImplementedException();
        }

        public AdminApproveUserResponse AdminApproveUser(int userId)
        {
            ApproveCalledCount++;
            return new AdminApproveUserResponse();
        }

        public AdminSuspendUserResponse AdminSuspendUser(int userId, int days, string reasonGiven)
        {
            SuspendCalledCount++;
            return new AdminSuspendUserResponse();
        }

        public AdminUnsuspendUserResponse AdminUnsuspendUser(int userId)
        {
            UnsuspendCalledCount++;
            return new AdminUnsuspendUserResponse();
        }

        public ReplyToPostResponse CreateReply(int category, int topicId, int? postId, string content)
        {
            throw new NotImplementedException();
        }

        public CreatePostResponse CreateTopic(int categoryId, string title, string content)
        {
            throw new NotImplementedException();
        }

        public GetTopicResponse GetTopic(int id)
        {
            throw new NotImplementedException();
        }

        public GetLatestTopicsResponse GetTopics()
        {
            throw new NotImplementedException();
        }

        public AdminGetUsersWithEmailResponse AdminGetUsers()
        {
            var result = new AdminGetUsersWithEmailResponse();
            result.Add(new DiscourseUser { Email = "test1@test.com", Approved = true}); // Do nothing
            result.Add(new DiscourseUser { Email = "test2@test.com", Approved = false}); //Approve
            result.Add(new DiscourseUser { Email = "test3@test.com", Approved = true, Suspended = true});// Reactivate
            result.Add(new DiscourseUser { Email = "layoric+u15@gmail.com", Approved = true}); //Suspend
            return result;
        }
    }
}
