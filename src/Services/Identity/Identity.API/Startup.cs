using System;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Identity.API;
using Identity.API.Data;
using IdentityServer4.Services;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.ServiceFabric;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopOnContainers.Services.Identity.API.Certificates;
using Microsoft.eShopOnContainers.Services.Identity.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Microsoft.Extensions.HealthChecks;

namespace Microsoft.eShopOnContainers.Services.Identity.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            #region Application Insights

            services.AddApplicationInsightsTelemetry(Configuration);
            var orchestratorType = Configuration.GetValue<string>("OrchestratorType");

            if (orchestratorType?.ToUpper() == "K8S")
            {
                // Enable K8s telemetry initializer
                services.EnableKubernetes();
            }
            if (orchestratorType?.ToUpper() == "SF")
            {
                // Enable SF telemetry initializer
                services.AddSingleton<ITelemetryInitializer>((serviceProvider) =>
                    new FabricTelemetryInitializer());
            }

            #endregion

            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
             options.UseSqlServer(Configuration["ConnectionString"],
                                     sqlServerOptionsAction: sqlOptions =>
                                     {
                                         sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                                         //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                                         sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                                     }));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<AppSettings>(Configuration);

            services.AddMvc();

            if (Configuration.GetValue<string>("IsClusterEnv") == bool.TrueString)
            {
                services.AddDataProtection(opts =>
                {
                    opts.ApplicationDiscriminator = "eshop.identity";
                })
                .PersistKeysToRedis(ConnectionMultiplexer.Connect(Configuration["DPConnectionString"]), "DataProtection-Keys");
            }

            services.AddHealthChecks(checks =>
            {
                var minutes = 1;
                if (int.TryParse(Configuration["HealthCheck:Timeout"], out var minutesParsed))
                {
                    minutes = minutesParsed;
                }
                checks.AddSqlCheck("Identity_Db", Configuration["ConnectionString"], TimeSpan.FromMinutes(minutes));
            });

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddTransient<ILoginService<ApplicationUser>, EFLoginService>();
            services.AddTransient<IRedirectService, RedirectService>();

            var connectionString = Configuration["ConnectionString"];
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            // Adds IdentityServer
            services.AddIdentityServer(x => x.IssuerUri = "null")
                .AddSigningCredential(Certificate.Get())
                .AddAspNetIdentity<ApplicationUser>()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString,
                                     sqlServerOptionsAction: sqlOptions =>
                                     {
                                         sqlOptions.MigrationsAssembly(migrationsAssembly);
                                         //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                                         sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                                     });
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString,
                                    sqlServerOptionsAction: sqlOptions =>
                                    {
                                        sqlOptions.MigrationsAssembly(migrationsAssembly);
                                         //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                                         sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                                    });
                })
                .Services.AddTransient<IProfileService, ProfileService>();

            services.AddAuthentication().AddFacebook(
                p => {
                    p.AppId = "132774750877803";
                    p.AppSecret = "8a8e93436265cdc5ffe9026cce8a13d9";
                    p.SaveTokens = true;
                    //p.Scope = 
                }).AddTwitter(
                p => {
                    p.ConsumerKey = "HccoQxUmO7nYBBW26E3Icy849";
                    p.ConsumerSecret = "zWyIzOHUleJrGG76GKM1jpCvItGEGCX8YFKEVTMKK27AU5OTK6";
                    p.SaveTokens = true;
                }).AddFoursquare(
                p => {
                    p.ClientId = "Z4JJKPXNTMLUN0UBRJ5OUK33Q5ZX4WH1WJYJGHEOSDL0PFA2";
                    p.ClientSecret = "VFWKO1WJVWOE4MLTX45I2ZVJKCV2FHAA0BLP2C2SSQ4Q2S3P";
                    p.SaveTokens = true;
                }).AddInstagram(
                p => {
                    p.ClientId = "ba32a692f0554e908a256dfc690d7d7f";
                    p.ClientSecret = "c612efca1631409bae1b6d241cee72ff";
                    p.SaveTokens = true;
                })

                .AddOAuth("Pinterest", "Pinterest", p =>
                {
                    p.ClientId = "4952518712832832384";
                    p.ClientSecret = "c612efca1631409bae1b6d241cee72ff";
                    p.AuthorizationEndpoint = "https://api.pinterest.com/oauth/";
                    p.TokenEndpoint = "https://api.pinterest.com/oauth/token";
                    p.UserInformationEndpoint = "https://api.pinterest.com/v1/me";
                    p.CallbackPath = new PathString("/signin-pinterest");
                    p.ClaimsIssuer = "Pinterest";
                    p.SaveTokens = true;
                })

                ;


            var container = new ContainerBuilder();
            container.Populate(services);

            return new AutofacServiceProvider(container.Build());
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        //public IServiceProvider ConfigursdfeServices(IServiceCollection services)
        //{
        //    var appSettings = this.Configuration.Get<AppSettings>();

        //    return services.AddRewsoAppInsights(this.Configuration)

        //                   // Add framework services.
        //                   .AddRewsoApplicationDbContext(this.Configuration["ConnectionString"])



        //            .AddSuppresedChain(svcs => 
        //            svcs.AddIdentity<ApplicationUser, IdentityRole>(), idBuilder =>idBuilder
        //                .AddEntityFrameworkStores<ApplicationDbContext>()
        //                .AddDefaultTokenProviders()
        //            )
            

        //            .Configure<AppSettings>(this.Configuration)
        //            .AddSuppresedChain(MvcServiceCollectionExtensions.AddMvc)


        //            .AddRedisDataProtection(this.Configuration)
        //            .AddRewsoHealthChecks(this.Configuration["ConnectionString"], this.Configuration.GetValue<TimeSpan>("HealthCheck:Timeout"))

        //            .AddTransient<IEmailSender, AuthMessageSender>()
        //            .AddTransient<ISmsSender, AuthMessageSender>()
        //            .AddTransient<ILoginService<ApplicationUser>, EFLoginService>()
        //            .AddTransient<IRedirectService, RedirectService>()

        //            .AddRewsoSts(this.Configuration)
        //            .BuildAutofacServiceProvider();
        //}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddAzureWebAppDiagnostics();
            loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Trace);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                
                
            }
            else
                app.UseExceptionHandler("/Home/Error");

            var pathBase = this.Configuration["PATH_BASE"];
            if (!string.IsNullOrEmpty(pathBase))
            {
                loggerFactory.CreateLogger("init").LogDebug($"Using PATH BASE '{pathBase}'");
                app.UsePathBase(pathBase);
            }


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            app.Map("/liveness", lapp => lapp.Run(async ctx => ctx.Response.StatusCode = 200));
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

            app.UseStaticFiles();


            // Make work identity server redirections in Edge and lastest versions of browers. WARN: Not valid in a production environment.
            app.Use(
                async (context, next) =>
                {
                    context.Response.Headers.Add("Content-Security-Policy", "script-src 'unsafe-inline'");
                    await next();
                });

            // Adds IdentityServer
            app.UseIdentityServer();

            app.UseMvc(routes => { routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}"); });
        }


    }
}
