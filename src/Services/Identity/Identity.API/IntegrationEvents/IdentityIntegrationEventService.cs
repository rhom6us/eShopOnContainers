using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.API.IntegrationEvents.Events;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;

namespace Identity.API.IntegrationEvents
{
    public class IdentityIntegrationEventService : IIdentityIntegrationEventService
    {
        private readonly IEventBus _eventBus;
        public IdentityIntegrationEventService(IEventBus eventBus) {
            _eventBus = eventBus;
        }

        Task IIdentityIntegrationEventService.AccessTokenReceived(string userId, string provider, string applicationId, string providerId, string accessToken) {
            var evt = new AccessTokenReceived(userId, provider, applicationId, providerId, accessToken);

            _eventBus.Publish(evt);
            return Task.FromResult<object>(null);
        }
    }
}
