using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscourseAPIClient;
using DiscourseAutoApprove.ServiceInterface;
using DiscourseAutoApprove.ServiceModel;
using Funq;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Logging;
using ServiceStack.Support;
using ServiceStack.Testing;

namespace DiscourseAutoApprove.Tests
{
    
    public class DiscourseClientTests
    {
        private readonly ServiceStackHost appHost;

        public DiscourseClientTests()
        {
            appHost = new BasicAppHost(typeof(SyncAccountServices).Assembly)
            {
                ConfigureContainer = ConfigureAppHost
            };
            SeedAppSettings();
            appHost.Init();
        }

        private void SeedAppSettings()
        {
            var customSettings = new FileInfo(@"~/../appsettings.txt".MapHostAbsolutePath());
            var appSettings = new TextFileSettings(customSettings.FullName);
            foreach (var key in appSettings.GetAllKeys())
            {
                appHost.AppSettings.Set(key,appSettings.Get(key));
            }
        }

        private void ConfigureAppHost(Container container)
        {
            LogManager.LogFactory = new InMemoryLogFactory();
            container.Register(appHost.AppSettings);
            container.Register<IDiscourseClient>(
                new DiscourseClient(
                    appHost.AppSettings.GetString("DiscourseRemoteUrl"),
                    appHost.AppSettings.GetString("DiscourseAdminApiKey"),
                    appHost.AppSettings.GetString("DiscourseAdminUserName")));
            container.Resolve<IDiscourseClient>().Login(
                appHost.AppSettings.GetString("DiscourseAdminUserName"), 
                appHost.AppSettings.GetString("DiscourseAdminPassword"));
            container.Register<IServiceStackAccountClient>(new MockServiceStackAccountClient());
        }

        [NUnit.Framework.Ignore("WARN: Actively updated Discourse")]
        [Test]
        public void TestSuspendAndUnsuspendUser()
        {
            var discourseClient = appHost.Resolve<IDiscourseClient>();
            discourseClient.AdminSuspendUser(4, 1, "This is a test");
            var testUser = discourseClient.AdminGetUsers().Where(x => x.Username == "archiveuser").FirstNonDefault();
            Assert.That(testUser != null);
            Assert.That(testUser.Id == 4);
            Assert.That(testUser.Suspended == true);

            discourseClient.AdminUnsuspendUser(4);
            testUser = discourseClient.AdminGetUsers().Where(x => x.Username == "archiveuser").FirstNonDefault();
            Assert.That(testUser != null);
            Assert.That(testUser.Id == 4);
            Assert.That(testUser.Suspended == null || testUser.Suspended == false);
        }


        [Test]
        public void TestAdminGetUsers()
        {
            var discourseClient = appHost.Resolve<IDiscourseClient>();
            var testUser = discourseClient.AdminGetUsers(limit:1000).Where(x => x.Username == "archiveuser").FirstNonDefault();
            Assert.That(testUser != null);
            Assert.That(testUser.Id == 4);
        }

        [Test]
        public void TestAdminGetUserById()
        {
            var discourseClient = appHost.Resolve<IDiscourseClient>();
            var testUser = discourseClient.GetUserById("mythz");
            Assert.That(testUser.User.Username, Is.EqualTo("mythz"));
            Assert.That(testUser.User.Name, Is.EqualTo("Demis Bellot"));
            Assert.That(testUser.User.Approved, Is.EqualTo(true));
            Assert.That(testUser.User.Moderator, Is.EqualTo(true));
        }

        [Test]
        public void TestFindUsersByFilter()
        {
            var discourseClient = appHost.Resolve<IDiscourseClient>();
            var testUsers = discourseClient.AdminFindUsersByFilter("layoric");
            var testUser = testUsers.FirstOrDefault();
            Assert.That(testUser, Is.Not.Null);
            Assert.That(testUser.Username, Is.EqualTo("layoric"));
            Assert.That(testUser.Approved, Is.EqualTo(true));
            Assert.That(testUser.Moderator, Is.EqualTo(true));
        }
    }
}
