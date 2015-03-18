using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using DiscourseAPIClient;
using Funq;
using DiscourseHookTest.ServiceInterface;
using DiscourseHookTest.ServiceModel;
using DiscourseHookTest.ServiceModel.Types;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Data;
using ServiceStack.Logging;
using ServiceStack.Logging.EventLog;
using ServiceStack.OrmLite;
using ServiceStack.Razor;
using ServiceStack.Text;

namespace DiscourseHookTest
{
    public class AppHost : AppHostBase
    {
        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public AppHost()
            : base("DiscourseHookTest", typeof(MyServices).Assembly)
        {
            var customSettings = new FileInfo(@"~/appsettings.txt".MapHostAbsolutePath());
            AppSettings = customSettings.Exists
                ? (IAppSettings)new TextFileSettings(customSettings.FullName)
                : new AppSettings();
        }

        /// <summary>
        /// Application specific configuration
        /// This method should initialize any IoC resources utilized by your web service classes.
        /// </summary>
        /// <param name="container"></param>
        public override void Configure(Container container)
        {
            //Config examples
            //this.Plugins.Add(new PostmanFeature());
            //this.Plugins.Add(new CorsFeature());

            SetConfig(new HostConfig
            {
                DebugMode = AppSettings.Get("DebugMode", false),
                AddRedirectParamsToQueryString = true
            });

            LogManager.LogFactory = new EventLogFactory("DiscourseAutoApprover","Application");

            this.Plugins.Add(new RazorFormat());
            container.Register(AppSettings);

            var client = new DiscourseClient(
                AppSettings.Get("DiscourseRemoteUrl", ""),
                AppSettings.Get("DiscourseAdminApiKey", ""),
                AppSettings.Get("DiscourseAdminUserName", ""));
            client.Login(AppSettings.Get("DiscourseAdminUserName", ""), AppSettings.Get("DiscourseAdminPassword", ""));
            container.Register<IDiscourseClient>(client);

            var serviceStackAccountClient = new ServiceStackAccountClient(AppSettings.GetString("ServiceStackCheckSubscriptionUrl"));
            container.Register<IServiceStackAccountClient>(serviceStackAccountClient);
        }
    }
}