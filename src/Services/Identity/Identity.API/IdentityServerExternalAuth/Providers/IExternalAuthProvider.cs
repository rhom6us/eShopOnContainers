using Newtonsoft.Json.Linq;

namespace Identity.API.IdentityServerExternalAuth.Providers
{
    public interface IExternalAuthProvider 
    {

        JObject GetUserInfo(string accessToken);

        string ProviderKey { get; }
    }
}