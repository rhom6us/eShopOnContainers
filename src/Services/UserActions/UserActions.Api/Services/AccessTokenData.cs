using System.Linq;

namespace UserActions.Api.Services {
    public class AccessTokenData {
        public string UserId { get; set; }
        public string SurrogateId { get; set; }
        public  string Provider { get; set; } 
        public string ApplicationId { get; set; }

        public string AccessToken { get; set; }
    }
    
}