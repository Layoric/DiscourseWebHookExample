using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using Funq;
using DiscourseHookTest.ServiceInterface;
using DiscourseHookTest.ServiceModel;
using DiscourseHookTest.ServiceModel.Types;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Data;
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

            this.Plugins.Add(new RazorFormat());
            container.Register(AppSettings);
            container.Register<IDbConnectionFactory>(new OrmLiteConnectionFactory("~/App_Data/db.sqlite".MapHostAbsolutePath(),
                SqliteDialect.Provider));

            using (var db = container.Resolve<IDbConnectionFactory>().OpenDbConnection())
            {
                db.CreateTableIfNotExists<ServiceStackCustomer>();
                if (db.Count<ServiceStackCustomer>() == 0)
                {
                    db.Insert(new ServiceStackCustomer
                    {
                        Email = "layoric+utest1@gmail.com",
                        SubscriptionExpiry = new DateTime(2015, 6, 1)
                    });
                    db.Insert(new ServiceStackCustomer
                    {
                        Email = "layoric+utest2@gmail.com",
                        SubscriptionExpiry = new DateTime(2015, 6, 1)
                    }); db.Insert(new ServiceStackCustomer
                    {
                        Email = "layoric+utest3@gmail.com",
                        SubscriptionExpiry = new DateTime(2015, 6, 1)
                    }); db.Insert(new ServiceStackCustomer
                    {
                        Email = "layoric+utest4@gmail.com",
                        SubscriptionExpiry = new DateTime(2015, 6, 1)
                    });
                }
            }
        }
    }
}