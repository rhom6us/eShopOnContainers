using System;
using System.Threading.Tasks;
using eShopOnContainers.Core.Services.RequestProvider;

namespace eShopOnContainers.Core.Services.Location
{
    public class LocationService : ILocationService
    {
        private readonly IRequestProvider _requestProvider;

        public LocationService(IRequestProvider requestProvider)
        {
            _requestProvider = requestProvider;
        }

        public async Task UpdateUserLocation(eShopOnContainers.Core.Models.Location.Location newLocReq, string token)
        {
            var builder = new UriBuilder(GlobalSetting.Instance.LocationEndpoint);
            builder.Path = "api/v1/locations";
            var uri = builder.ToString();
            await _requestProvider.PostAsync(uri, newLocReq, token);
        }
    }
}