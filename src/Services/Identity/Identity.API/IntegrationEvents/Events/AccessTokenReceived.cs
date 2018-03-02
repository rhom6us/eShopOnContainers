using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Events;

namespace Identity.API.IntegrationEvents.Events
{
    [DataContract]
    public class AccessTokenReceived : IntegrationEvent
    {
        [DataMember]
        public string UserId { get; private set; }
        [DataMember]
        public string ProviderId { get; private set; }
        [DataMember]
        public string ApplicationId { get; private set; }
        [DataMember]
        public string Provider { get; private set; }
        [DataMember]
        public string AccessToken { get; private set; }
        public AccessTokenReceived(string userId, string provider, string applicationId, string providerId, string accessToken) {
            this.Provider = provider;
            this.AccessToken = accessToken;
            this.ApplicationId = applicationId;
            this.ProviderId = providerId;
            this.UserId = userId;
        }
    }
}
