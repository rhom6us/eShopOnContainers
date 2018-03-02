using System.Threading.Tasks;

namespace Identity.API.IntegrationEvents
{
    public interface IIdentityIntegrationEventService {
        Task AccessTokenReceived(string userId, string provider, string applicationId, string providerId, string accessToken);
    }
}