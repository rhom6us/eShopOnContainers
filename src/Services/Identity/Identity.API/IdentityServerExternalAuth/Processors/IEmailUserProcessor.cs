using System.Threading.Tasks;
using IdentityServer4.Validation;
using Newtonsoft.Json.Linq;

namespace IdentityServerExternalAuth.Interfaces.Processors
{
    public interface IEmailUserProcessor
    {
        Task<GrantValidationResult> ProcessAsync(JObject userInfo, string email, string provider);
    }
}