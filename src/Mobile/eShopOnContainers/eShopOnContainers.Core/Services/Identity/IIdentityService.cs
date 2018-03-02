using eShopOnContainers.Core.Models.Token;
using System.Threading.Tasks;

namespace eShopOnContainers.Core.Services.Identity
{
    public interface IIdentityService
    {
        string CreateAuthorizationRequest();
        Task<string> ExchangeToken(string token);
        string CreateLogoutRequest(string token);
        Task<UserToken> GetTokenAsync(string code);
    }
}