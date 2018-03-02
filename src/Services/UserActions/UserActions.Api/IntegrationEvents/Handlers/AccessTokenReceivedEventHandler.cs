using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserActions.Api.IntegrationEvents.Events;
using UserActions.Api.Services;

namespace UserActions.Api.IntegrationEvents.Handlers {
    public class AccessTokenReceivedEventHandler : IIntegrationEventHandler<AccessTokenReceived> {
        private readonly IOptionsSnapshot<AppSettings> _appSettings;
        public AccessTokenReceivedEventHandler([NotNull] IAccessTokenRepository accessTokenRepository, [NotNull] ILoggerFactory loggerFactory, IOptionsSnapshot<AppSettings> appSettings) {
            _accessTokenRepository = accessTokenRepository;
            _appSettings = appSettings;
            _logger = loggerFactory.CreateLogger<AccessTokenReceivedEventHandler>();
        }


        public Task Handle([NotNull] AccessTokenReceived @event) {
            _logger.LogDebug("Begin AccessTokenReceived handler");
            
            return _accessTokenRepository.UpdateAccessTokenAsync(@event.Provider, @event.ApplicationId, @event.ProviderId, @event.AccessToken);
        }
        [CanBeNull]
        private string GetAppId([NotNull] string provider) {
            return _appSettings.Value.Authentication[provider]?.ApplicationId;
        }

        private readonly IAccessTokenRepository _accessTokenRepository;
        private readonly ILogger<AccessTokenReceivedEventHandler> _logger;
    }
}
