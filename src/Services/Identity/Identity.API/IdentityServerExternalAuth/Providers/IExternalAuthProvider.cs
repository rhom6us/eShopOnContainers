using Newtonsoft.Json.Linq;

namespace IdentityServerExternalAuth.Interfaces
{
    public interface IExternalAuthProvider
    {
        JObject GetUserInfo(string accessToken);
    }
}