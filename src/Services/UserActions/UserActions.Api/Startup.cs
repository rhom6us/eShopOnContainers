using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.ServiceFabric;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBusRabbitMQ;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBusServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.HealthChecks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Swashbuckle.AspNetCore.Swagger;
using UserActions.Api.Infrastructure.Filters;
using UserActions.Api.Infrastructure.Middleware;
using UserActions.Api.IntegrationEvents.Events;
using UserActions.Api.IntegrationEvents.Handlers;
using UserActions.Api.Services;

namespace UserActions.Api {
    public class Startup {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) => this.Configuration = configuration;

        public static void Main(string[] args) => Startup.BuildWebHost(args).Run();

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseHealthChecks("/hc")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((builderContext, config) => config.AddEnvironmentVariables())
                .ConfigureLogging((hostingContext, builder) => {
                    builder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .UseApplicationInsights()
                .Build();

        // This method gets called by the runtime. Use this method to add services to the container.
        [JetBrains.Annotations.NotNull]
        public IServiceProvider ConfigureServices(IServiceCollection services) {
            services.Configure<AppSettings>(Configuration);
            services.AddMvc();

            if (this.Configuration.GetValue<bool>("AzureServiceBusEnabled")) {
                services.AddSingleton<IServiceBusPersisterConnection>(sp => {
                    var logger = sp.GetRequiredService<ILogger<DefaultServiceBusPersisterConnection>>();

                    var serviceBusConnectionString = this.Configuration["EventBusConnection"];
                    var serviceBusConnection = new ServiceBusConnectionStringBuilder(serviceBusConnectionString);

                    return new DefaultServiceBusPersisterConnection(serviceBusConnection, logger);
                });
            } else {
                services.AddSingleton<IRabbitMQPersistentConnection>(sp => {
                    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                    var factory = new ConnectionFactory {HostName = this.Configuration["EventBusConnection"]};

                    if (!string.IsNullOrEmpty(this.Configuration["EventBusUserName"]))
                        factory.UserName = this.Configuration["EventBusUserName"];

                    if (!string.IsNullOrEmpty(this.Configuration["EventBusPassword"]))
                        factory.Password = this.Configuration["EventBusPassword"];

                    var retryCount = 5;
                    if (!string.IsNullOrEmpty(this.Configuration["EventBusRetryCount"]))
                        retryCount = int.Parse(this.Configuration["EventBusRetryCount"]);

                    return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
                });
            }

            services.AddHealthChecks(checks => { checks.AddValueTaskCheck("HTTP Endpoint", () => new ValueTask<IHealthCheckResult>(HealthCheckResult.Healthy("Ok"))); });

            this.RegisterEventBus(services);

            // Add framework services.
            services.AddSwaggerGen(options => {
                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1",
                    new Info {
                        Title = "eShopOnContainers - Location HTTP API",
                        Version = "v1",
                        Description = "The Location Microservice HTTP API. This is a Data-Driven/CRUD microservice sample",
                        TermsOfService = "Terms Of Service"
                    });

                options.AddSecurityDefinition("oauth2",
                    new OAuth2Scheme {
                        Type = "oauth2",
                        Flow = "implicit",
                        AuthorizationUrl = $"{this.Configuration.GetValue<string>("IdentityUrlExternal")}/connect/authorize",
                        TokenUrl = $"{this.Configuration.GetValue<string>("IdentityUrlExternal")}/connect/token",
                        Scopes = new Dictionary<string, string> {{"locations", "Locations API"}}
                    });

                options.OperationFilter<AuthorizeCheckOperationFilter>();
            });

            services.AddCors(options => {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTransient<AccessTokenReceivedEventHandler>();

            services.BuildServiceProvider();
            var container = new ContainerBuilder();
            container.Populate(services);
            return new AutofacServiceProvider(container.Build());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
            loggerFactory.AddAzureWebAppDiagnostics();
            loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Trace);

            var pathBase = this.Configuration["PATH_BASE"];
            if (!string.IsNullOrEmpty(pathBase))
                app.UsePathBase(pathBase);

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            app.Map("/liveness", lapp => lapp.Run(async ctx => ctx.Response.StatusCode = 200));
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

            app.UseCors("CorsPolicy");

            this.ConfigureAuth(app);

            app.UseMvcWithDefaultRoute();

            app.UseSwagger()
                .UseSwaggerUI(c => {
                    c.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}/swagger/v1/swagger.json", "UserActions.API V1");
                    c.ConfigureOAuth2("useractionsswaggerui", "", "", "User Actions Swagger UI");
                });
            ConfigureEventBus(app);
            //LocationsContextSeed.SeedAsync(app, loggerFactory).Wait();
        }

        private void RegisterAppInsights(IServiceCollection services) {
            services.AddApplicationInsightsTelemetry(this.Configuration);
            var orchestratorType = this.Configuration.GetValue<string>("OrchestratorType");

            if (orchestratorType?.ToUpper() == "K8S") {
                // Enable K8s telemetry initializer
                services.EnableKubernetes();
            }

            if (orchestratorType?.ToUpper() == "SF") {
                // Enable SF telemetry initializer
                services.AddSingleton<ITelemetryInitializer>(serviceProvider =>
                    new FabricTelemetryInitializer());
            }
        }

        private void ConfigureAuthService(IServiceCollection services) {
            // prevent from mapping "sub" claim to nameidentifier.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options => {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options => {
                    options.Authority = this.Configuration.GetValue<string>("IdentityUrl");
                    options.Audience = "locations";
                    options.RequireHttpsMetadata = false;
                });
        }

        protected virtual void ConfigureAuth(IApplicationBuilder app) {
            if (this.Configuration.GetValue<bool>("UseLoadTest"))
                app.UseMiddleware<ByPassAuthMiddleware>();

            app.UseAuthentication();
        }

        private void RegisterEventBus(IServiceCollection services) {
            var subscriptionClientName = this.Configuration[key: "SubscriptionClientName"];

            if (this.Configuration.GetValue<bool>(key: "AzureServiceBusEnabled")) {
                services.AddSingleton<IEventBus, EventBusServiceBus>(implementationFactory: sp => new EventBusServiceBus(
                    serviceBusPersisterConnection: sp.GetRequiredService<IServiceBusPersisterConnection>(),
                    logger: sp.GetRequiredService<ILogger<EventBusServiceBus>>(),
                    subsManager: sp.GetRequiredService<IEventBusSubscriptionsManager>(),
                    subscriptionClientName: subscriptionClientName,
                    autofac: sp.GetRequiredService<ILifetimeScope>()));
            } else {
                services.AddSingleton<IEventBus, EventBusRabbitMQ>(implementationFactory: sp => {
                    var rabbitMqPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                    var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                    var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                    var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                    var retryCount = 5;
                    if (!string.IsNullOrEmpty(value: this.Configuration[key: "EventBusRetryCount"]))
                        retryCount = int.Parse(s: this.Configuration[key: "EventBusRetryCount"]);

                    return new EventBusRabbitMQ(persistentConnection: rabbitMqPersistentConnection, logger: logger, autofac: iLifetimeScope, subsManager: eventBusSubcriptionsManager, queueName: subscriptionClientName, retryCount: retryCount);
                });
            }

            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>()
                .AddSingleton<IAccessTokenRepository, AccessTokenRepository>()
                ;
        }
        protected virtual void ConfigureEventBus(IApplicationBuilder app) {
            app.ApplicationServices.GetRequiredService<IEventBus>()
                .Subscribe<AccessTokenReceived, AccessTokenReceivedEventHandler>();
        }
    }
}
