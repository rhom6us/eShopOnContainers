using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.API.Configuration;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Identity.API.Data {

    public class ConfigurationDbContextSeed {
        public async Task SeedAsync(ConfigurationDbContext context, IConfiguration configuration) {
            //callbacks urls from config:
            var clientUrls = new Dictionary<string, string> {
                {"Mvc", configuration.GetValue<string>("MvcClient")},
                {"Spa", configuration.GetValue<string>("SpaClient")},
                {"Xamarin", configuration.GetValue<string>("XamarinCallback")},
                {"LocationsApi", configuration.GetValue<string>("LocationApiClient")},
                {"MarketingApi", configuration.GetValue<string>("MarketingApiClient")},
                {"BasketApi", configuration.GetValue<string>("BasketApiClient")},
                {"OrderingApi", configuration.GetValue<string>("OrderingApiClient")}
            };

                context.Clients.RemoveRange(context.Clients);
            await context.Clients.AddRangeAsync(Config.GetClients(clientUrls).Select(ClientMappers.ToEntity));

                context.IdentityResources.RemoveRange(context.IdentityResources);
            await context.IdentityResources.AddRangeAsync(Config.GetResources().Select(IdentityResourceMappers.ToEntity));

                context.ApiResources.RemoveRange(context.ApiResources);
            await context.ApiResources.AddRangeAsync(Config.GetApis().Select(ApiResourceMappers.ToEntity));

            
            
            await context.SaveChangesAsync();
        }
    }
}
