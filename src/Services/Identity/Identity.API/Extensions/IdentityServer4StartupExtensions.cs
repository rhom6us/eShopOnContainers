using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Identity.API.Data;
using Identity.API.Migrations;
using IdentityServer4.Services;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.ServiceFabric;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.eShopOnContainers.Services.Identity.API;
using Microsoft.eShopOnContainers.Services.Identity.API.Certificates;
using Microsoft.eShopOnContainers.Services.Identity.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.HealthChecks;
using StackExchange.Redis;
using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection {
    internal static class RewsoStartupExteneions

    {
        public static IServiceCollection AddRewsoApplicationDbContext(this IServiceCollection services, string connectionString) {
            return
                services.AddDbContext<ApplicationDbContext>(
                    options =>
                        options.UseSqlServer(
                            connectionString,
                            sqlOptions => sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name)
                            .EnableRetryOnFailure(15, TimeSpan.FromSeconds(30), null)
                            //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                        ));
        }

        public static IServiceCollection AddOrchestrator(this IServiceCollection services, string orchestratorType) {
            switch (orchestratorType?.ToUpper()) {
                case "K8S":
                    return services.EnableKubernetes();
                case "SF":
                    return services.AddSingleton<ITelemetryInitializer>(serviceProvider => new FabricTelemetryInitializer());
                default:
                    return services;
            }
        }

        public static IServiceCollection AddRewsoAppInsights(this IServiceCollection services, IConfiguration configuration) {
            return services.AddApplicationInsightsTelemetry(configuration).AddOrchestrator(configuration.GetValue<string>("OrchestratorType"));
        }


        public static IServiceCollection AddRewsoHealthChecks(this IServiceCollection services, string identityDbConnectionString, TimeSpan? timeout) {
            return services.AddHealthChecks(
                checks => checks.AddSqlCheck("Identity_Db", identityDbConnectionString, timeout.GetValueOrDefault())
                );
        }

        public static IServiceCollection AddRedisDataProtection(this IServiceCollection services, IConfiguration configuration) {
            if (configuration.GetValue<string>("IsClusterEnv") == bool.TrueString) {
                services.AddDataProtection(opts => { opts.ApplicationDiscriminator = "eshop.identity"; })
                        .PersistKeysToRedis(ConnectionMultiplexer.Connect(configuration["DPConnectionString"]), "DataProtection-Keys");
            }
            return services;
        }

        public static IServiceCollection AddRewsoSts(this IServiceCollection services, IConfiguration configuration) {
            var migrationsAssembly = typeof(ApplicationDbContextModelSnapshot).GetTypeInfo().Assembly.GetName().Name;
            var connectionString = configuration["ConnectionString"];

            void ConfigureDb(DbContextOptionsBuilder builder)
            {
                 builder.UseSqlServer(connectionString,
                    sqlOptions => {
                        sqlOptions.MigrationsAssembly(migrationsAssembly);
                            //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                            sqlOptions.EnableRetryOnFailure(15, TimeSpan.FromSeconds(30), null);
                    });
            }

            services.AddIdentityServer(
                        x => x.IssuerUri = "null",
                        builder => 
                            builder.AddSigningCredential(Certificate.Get())
                            .AddAspNetIdentity<ApplicationUser>()
                                   .AddConfigurationStore(options => { options.ConfigureDbContext = ConfigureDb; })
                                   .AddOperationalStore(options => { options.ConfigureDbContext = ConfigureDb; })
                        )
                    .AddTransient<IProfileService, ProfileService>()
                    //.AddIdentity<ApplicationUser, IdentityRole>(
                    //    options => {
                    //        options.Password.RequiredUniqueChars = 1;
                    //        options.Password.RequiredLength = 1;
                    //        options.SignIn.RequireConfirmedEmail = false;
                    //        options.SignIn.RequireConfirmedPhoneNumber = false;
                    //        options.Password.RequireDigit = false;
                    //        options.Password.RequireLowercase = false;
                    //        options.Password.RequireNonAlphanumeric = false;
                    //        options.Password.RequireUppercase = false;
                    //        options.User.RequireUniqueEmail = false;
                    //        //options.User.AllowedUserNameCharacters = 
                    //        options.Lockout.AllowedForNewUsers = false;
                    //        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                    //        options.Lockout.MaxFailedAccessAttempts = 5;

                    //    }).AddDefaultTokenProviders()
                        ;

            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
                    .AddCookie()
                .AddFacebook(p => {
                    p.AppId = "132774750877803";
                    p.AppSecret = "8a8e93436265cdc5ffe9026cce8a13d9";
                    p.SaveTokens = true;
                    //p.Scope = 
                });

            return services;


            //.AddTwitter( p => {
            //        p.ConsumerKey = configuration["Authentication:Twitter:AppId"];
            //        p.ConsumerSecret = configuration["Authentication:Twitter:AppSecret"];
            //        p.SaveTokens = true;
            //})
            //.AddInstagram( p => {
            //    p.ClientId = configuration["Authentication:Instagram:AppId"];
            //    p.ClientSecret = configuration["Authentication:Instagram:AppSecret"];
            //    p.SaveTokens = true;
            //})
            //.AddFoursquare( p => {
            //    p.ClientId = configuration["Authentication:Foursquare:AppId"];
            //    p.ClientSecret = configuration["Authentication:Foursquare:AppSecret"];
            //    p.SaveTokens = true;
            //})
            //.AddOAuth("Pinterest", "Pinterest", p => {
            //    p.ClientId = configuration["Authentication:Pinterest:AppId"];
            //    p.ClientSecret = configuration["Authentication:Pinterest:AppSecret"];
            //    p.AuthorizationEndpoint = "https://api.pinterest.com/oauth/";
            //    p.TokenEndpoint = "https://api.pinterest.com/oauth/token";
            //    p.UserInformationEndpoint = "https://api.pinterest.com/v1/me";

            //    p.SaveTokens = true;
            //})
            //.AddOAuth("Flickr", "Flickr", p => {
            //    p.ClientId = configuration["Authentication:Flickr:AppId"];
            //    p.ClientSecret = configuration["Authentication:Flickr:AppSecret"];
            //    p.AuthorizationEndpoint = "https://www.flickr.com/services/oauth/authorize";

            //    p.TokenEndpoint = "https://www.flickr.com/services/oauth/access_token";
            //    p.UserInformationEndpoint = "https://api.pinterest.com/v1/me";

            //    p.SaveTokens = true;
            //})
            // );



        }
    }
    
    public static class qwer
    {
        public static IServiceCollection AddAuthentication(this IServiceCollection services, Func<AspNetCore.Authentication.AuthenticationBuilder, AspNetCore.Authentication.AuthenticationBuilder> builder)
        {
            return builder(services.AddAuthentication()).Services;
        }
        public static IServiceCollection AddIdentityServer(this IServiceCollection services, Action<IdentityServerOptions> setupAction, Func<IIdentityServerBuilder, IIdentityServerBuilder> builder)
        {
            return builder(services.AddIdentityServer(setupAction)).Services;
            

        }
    }
}
