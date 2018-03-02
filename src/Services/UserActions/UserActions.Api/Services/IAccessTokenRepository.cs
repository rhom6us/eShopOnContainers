using System.Threading.Tasks;

namespace UserActions.Api.Services {
    public interface IAccessTokenRepository {
        Task<string> GetAccessTokenAsync(string provider, string applicationId, string surrogateId);
        Task UpdateAccessTokenAsync( string provider, string applicationId, string surrogateId, string accessToken);
        
    }
}