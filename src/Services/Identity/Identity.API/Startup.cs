using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Identity.API.Data;
using Identity.API.Extensions;
using Identity.API.IdentityServerExternalAuth;
using Identity.API.IdentityServerExternalAuth.Providers;
using Identity.API.IntegrationEvents;
using Identity.API.Services;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using JetBrains.Annotations;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.ServiceFabric;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.ServiceBus;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBusRabbitMQ;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBusServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace Identity.API
{

    internal static class ServiceCollectionExtensions
    {
        public static IWebHost BuildWebHost(this IWebHostBuilder builder) =>
            builder
                .UseKestrel()
                .UseHealthChecks("/hc")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .ConfigureAppConfiguration((builderContext, config) => { config.AddEnvironmentVariables().AddUserSecrets<Program>(); })
                .UseStartup<Startup>()
                .ConfigureLogging((hostingContext, loggerBuilder) => {
                    loggerBuilder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    loggerBuilder.AddConsole();
                    loggerBuilder.AddDebug();
                })
                .UseApplicationInsights()
                .Build();
        public static IWebHost MigrateDbs(this IWebHost webHost) {
            return webHost
                .MigrateDbContext<IdentityServer4.EntityFramework.DbContexts.PersistedGrantDbContext>((_, __) => { })
                .MigrateDbContext<ApplicationDbContext>((context, services) => {
                    var env = services.GetService<IHostingEnvironment>();
                    var logger = services.GetService<ILogger<ApplicationDbContextSeed>>();
                    var settings = services.GetService<IOptions<AppSettings>>();

                    new ApplicationDbContextSeed()
                        .SeedAsync(context, env, logger, settings)
                        .Wait();
                })
                .MigrateDbContext<ConfigurationDbContext>((context, services) => {
                    var configuration = services.GetService<IConfiguration>();

                    new ConfigurationDbContextSeed()
                        .SeedAsync(context, configuration)
                        .Wait();
                });
        }
        public static IServiceCollection AddOrchestration(this IServiceCollection services, [CanBeNull] string orchestratorType) {
            switch (orchestratorType?.ToUpper()) {
                case "K8S":
                    // Enable K8s telemetry initializer
                    return services.EnableKubernetes();
                case "SF":
                    // Enable SF telemetry initializer
                    return services.AddSingleton<ITelemetryInitializer>(serviceProvider => new FabricTelemetryInitializer());
            }

            return services;
        }
        public static IServiceCollection AddClusterEnv(this IServiceCollection services, IConfiguration configuration) {
            if (configuration.GetValue<string>("IsClusterEnv") == bool.TrueString) {
                return services.AddDataProtection(opts => {
                    opts.ApplicationDiscriminator = "eshop.identity";
                })
                .PersistKeysToRedis(
                    connectionMultiplexer: ConnectionMultiplexer.Connect(configuration["DPConnectionString"]),
                    key: "DataProtection-Keys"
                ).Services;
            }

            return services;
        }

        public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration) {
            return services.AddHealthChecks(checks => {
                var minutes = 1;
                if (int.TryParse(configuration["HealthCheck:Timeout"], out var minutesParsed))
                    minutes = minutesParsed;
                checks.AddSqlCheck("Identity_Db", configuration["ConnectionString"], TimeSpan.FromMinutes(minutes));
            });
        }

        public static IServiceCollection AddOidcServier(this IServiceCollection services, IConfiguration configuration) {
            // Adds IdentityServer
            var connectionString = configuration["ConnectionString"];
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            return services.AddIdentityServer(p => p.IssuerUri = "https://abeb159c.ngrok.io")
                .AddSigningCredential(new X509Certificate2(Properties.Resources.OidcSigningCredential, "idsrv3test"))

                .AddConfigurationStore<ConfigurationDbContext>(options => {
                    options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString,
                        sqlOptions => {
                            sqlOptions.MigrationsAssembly(migrationsAssembly);
                            //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                            sqlOptions.EnableRetryOnFailure(15, TimeSpan.FromSeconds(30), null);
                        });
                })
                .AddOperationalStore(options => {
                    options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString,
                        sqlOptions => {
                            sqlOptions.MigrationsAssembly(migrationsAssembly);
                            //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                            sqlOptions.EnableRetryOnFailure(15, TimeSpan.FromSeconds(30), null);
                        });
                })
                 .AddAspNetIdentity<ApplicationUser>()
                .Services
                .AddTransient<IProfileService, ProfileService>()
                .AddAuthentication()
                .AddFacebook(p => {
                    p.AppId = "132774750877803";
                    p.AppSecret = "8a8e93436265cdc5ffe9026cce8a13d9";
                    p.AppSecret = "8a8e93436265cdc5ffe9026cce8a13d9";
                    p.Scope.Add("email");
                    p.Scope.Add("pages_show_list");
                    p.Scope.Add("user_posts");
                    p.Scope.Add("user_tagged_places");
                    
                    p.Events.OnCreatingTicket = async context => {
                        context.Identity.AddClaim(new System.Security.Claims.Claim("urn:facebook:access_token", context.AccessToken));
                        context.Success();
                    };
                    p.Events.OnRedirectToAuthorizationEndpoint += async context => { };
                    p.SaveTokens = true;
                    //p.Scope = 
                })
                .AddTwitter(p => {
                    p.ConsumerKey = "HccoQxUmO7nYBBW26E3Icy849";
                    p.ConsumerSecret = "zWyIzOHUleJrGG76GKM1jpCvItGEGCX8YFKEVTMKK27AU5OTK6";
                    p.SaveTokens = true;
                })
                .AddFoursquare(p => {
                    p.ClientId = "Z4JJKPXNTMLUN0UBRJ5OUK33Q5ZX4WH1WJYJGHEOSDL0PFA2";
                    p.ClientSecret = "VFWKO1WJVWOE4MLTX45I2ZVJKCV2FHAA0BLP2C2SSQ4Q2S3P";
                    p.SaveTokens = true;
                })
                .AddInstagram(p => {
                    p.ClientId = "ba32a692f0554e908a256dfc690d7d7f";
                    p.ClientSecret = "c612efca1631409bae1b6d241cee72ff";
                    p.SaveTokens = true;
                })
                .AddOAuth("Pinterest",
                    "Pinterest",
                    p => {
                        p.ClientId = "4952518712832832384";
                        p.ClientSecret = "c612efca1631409bae1b6d241cee72ff";
                        p.AuthorizationEndpoint = "https://api.pinterest.com/oauth/";
                        p.TokenEndpoint = "https://api.pinterest.com/oauth/token";
                        p.UserInformationEndpoint = "https://api.pinterest.com/v1/me";
                        p.CallbackPath = new PathString("/signin-pinterest");
                        p.ClaimsIssuer = "Pinterest";
                        p.SaveTokens = true;
                    })
                .Services



            .AddCustomGrantExtension()

            ;

        }
        public static IServiceCollection AddCustomGrantExtension(this IServiceCollection services) {

            return services
                .AddScoped<IExtensionGrantValidator, ExternalAuthenticationGrant<ApplicationUser>>()
                .AddSingleton<HttpClient>()
                .AddScoped<IProviderRepository, ProviderRepository>()

                .AddTransient<FacebookAuthProvider>()
                .AddTransient<TwitterAuthProvider>()
                .AddTransient<IEnumerable<IExternalAuthProvider>>(p => new IExternalAuthProvider[] {
                        p.GetRequiredService<FacebookAuthProvider>(),
                        p.GetRequiredService<TwitterAuthProvider>(),
                    }.ToList()
                    .AsReadOnly());
            //.AddTransient<IGoogleAuthProvider, GoogleAuthProvider<ApplicationUser>>()
            //.AddTransient<ILinkedInAuthProvider, LinkedInAuthProvider<ApplicationUser>>()
            //.AddTransient<IGitHubAuthProvider, GitHubAuthProvider<ApplicationUser>>()

            ;

        }

        public static IServiceCollection RegisterEventBus(this IServiceCollection services, IConfiguration configuration) {
            var subscriptionClientName = configuration["SubscriptionClientName"];

            if (configuration.GetValue<bool>("AzureServiceBusEnabled")) {
                services.AddSingleton<IEventBus, EventBusServiceBus>(sp => {
                    var serviceBusPersisterConnection = sp.GetRequiredService<IServiceBusPersisterConnection>();
                    var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                    var logger = sp.GetRequiredService<ILogger<EventBusServiceBus>>();
                    var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                    return new EventBusServiceBus(serviceBusPersisterConnection, logger,
                        eventBusSubcriptionsManager, subscriptionClientName, iLifetimeScope);
                });

            }
            else {
                services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp => {
                    var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                    var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                    var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                    var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                    var retryCount = 5;
                    if (!string.IsNullOrEmpty(configuration["EventBusRetryCount"])) {
                        retryCount = int.Parse(configuration["EventBusRetryCount"]);
                    }

                    return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager, subscriptionClientName, retryCount);
                });
            }

            return services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
        }

        [NotNull]
        public static IApplicationBuilder ConfigureEventBus(this IApplicationBuilder app) {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            //eventBus.Subscribe<OrderStatusChangedToAwaitingValidationIntegrationEvent, OrderStatusChangedToAwaitingValidationIntegrationEventHandler>();
            // eventBus.Subscribe<OrderStatusChangedToPaidIntegrationEvent, OrderStatusChangedToPaidIntegrationEventHandler>();
            return app;
        }
    }

    internal static class WebHostBuilderExtensions
    {

    }

    public class Startup
    {
        public static void Main(string[] args) {
            WebHost.CreateDefaultBuilder(args)
                .BuildWebHost()
                .MigrateDbs()
                .Run();
        }

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) {
            this.Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        [NotNull]
        public IServiceProvider ConfigureServices(IServiceCollection services) {
            #region Application Insights

            services.AddApplicationInsightsTelemetry(this.Configuration);

            services.AddOrchestration(this.Configuration.GetValue<string>("OrchestratorType"));

            #endregion

            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(this.Configuration["ConnectionString"],
                    sqlOptions => {
                        sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                        //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                        sqlOptions.EnableRetryOnFailure(15, TimeSpan.FromSeconds(30), null);
                    }));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddOidcServier(this.Configuration);

            services.Configure<AppSettings>(this.Configuration);

            services.AddMvc();

            services.AddClusterEnv(this.Configuration);

            services.AddHealthChecks(this.Configuration);

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddTransient<ILoginService<ApplicationUser>, EFLoginService>();
            services.AddTransient<IRedirectService, RedirectService>();


            services.AddTransient<IIdentityIntegrationEventService, IdentityIntegrationEventService>();

            if (this.Configuration.GetValue<bool>("AzureServiceBusEnabled")) {
                services.AddSingleton<IServiceBusPersisterConnection>(sp => {
                    var settings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
                    var logger = sp.GetRequiredService<ILogger<DefaultServiceBusPersisterConnection>>();

                    var serviceBusConnection = new ServiceBusConnectionStringBuilder(settings.EventBusConnection);

                    return new DefaultServiceBusPersisterConnection(serviceBusConnection, logger);
                });
            }
            else {
                services.AddSingleton<IRabbitMQPersistentConnection>(sp => {
                    var settings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
                    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                    var factory = new ConnectionFactory() {
                        HostName = this.Configuration["EventBusConnection"]
                    };

                    if (!string.IsNullOrEmpty(this.Configuration["EventBusUserName"])) {
                        factory.UserName = this.Configuration["EventBusUserName"];
                    }

                    if (!string.IsNullOrEmpty(this.Configuration["EventBusPassword"])) {
                        factory.Password = this.Configuration["EventBusPassword"];
                    }

                    var retryCount = 5;
                    if (!string.IsNullOrEmpty(this.Configuration["EventBusRetryCount"])) {
                        retryCount = int.Parse(this.Configuration["EventBusRetryCount"]);
                    }

                    return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
                });
            }

            services.RegisterEventBus(Configuration);

            services.BuildAutofacServiceProvider();


            var container = new ContainerBuilder();
            container.Populate(services);

            return new AutofacServiceProvider(container.Build());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
            loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddAzureWebAppDiagnostics();
            loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Trace);

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
                app.UseExceptionHandler("/Home/Error");

            var pathBase = this.Configuration["PATH_BASE"];
            if (!string.IsNullOrEmpty(pathBase)) {
                loggerFactory.CreateLogger("init").LogDebug($"Using PATH BASE '{pathBase}'");
                app.UsePathBase(pathBase);
            }


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            app.Map("/liveness", lapp => lapp.Run(async ctx => ctx.Response.StatusCode = 200));
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

            app.UseStaticFiles();


            // Make work identity server redirections in Edge and lastest versions of browers. WARN: Not valid in a production environment.
            app.Use(async (context, next) => {
                context.Response.Headers.Add("Content-Security-Policy", "script-src 'unsafe-inline'");
                await next();
            });

            // Adds IdentityServer
            app.UseIdentityServer();

            app.UseMvc(routes => { routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}"); });

            app.ConfigureEventBus();
        }


    }
}
