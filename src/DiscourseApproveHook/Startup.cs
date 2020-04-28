using DiscourseAPIClient;
using DiscourseAutoApprove.ServiceInterface;
using DiscourseAutoApprove.ServiceModel;
using Funq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack;
using ServiceStack.FluentValidation;
using ServiceStack.Validation;

namespace DiscourseApproveHook
{
    public class Startup : ModularStartup
    {
        public Startup(IConfiguration configuration) : base(configuration) { }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public new void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseServiceStack(new AppHost
            {
                AppSettings = new NetCoreAppSettings(Configuration)
            });
        }
    }

    public partial class AppHost : AppHostBase
    {
        public AppHost() 
            : base($"ServiceStack Discourse WebHooks", typeof(DailyServices).Assembly) {}

        public static void Load() => PreInit();
        static partial void PreInit();
        static partial void PreConfigure(IAppHost appHost);

        public override void Configure(Container container)
        {
            PreConfigure(this);

            SetConfig(new HostConfig
            {
                DebugMode = AppSettings.Get("DebugMode", false),
                AddRedirectParamsToQueryString = true
            });


            Plugins.Add(new SharpPagesFeature());
            Plugins.Add(new ValidationFeature());
            container.Register(AppSettings);

            var client = new DiscourseClient(
                AppSettings.Get("DiscourseRemoteUrl", ""),
                AppSettings.Get("DiscourseAdminApiKey", ""),
                AppSettings.Get("DiscourseAdminUserName", ""));
            client.Login(AppSettings.Get("DiscourseAdminUserName", ""), AppSettings.Get("DiscourseAdminPassword", ""));
            container.Register<IDiscourseClient>(client);

            var serviceStackAccountClient = new ServiceStackAccountClient(AppSettings.GetString("CheckSubscriptionUrl"));
            container.Register<IServiceStackAccountClient>(serviceStackAccountClient);
        }
    }

    public class SyncSingleUserByEmailValidator : AbstractValidator<SyncSingleUserByEmail>
    {
        public SyncSingleUserByEmailValidator()
        {
            RuleFor(x => x.Email).EmailAddress();
        }
    }

}
