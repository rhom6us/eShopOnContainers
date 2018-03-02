using Newtonsoft.Json;

namespace eShopOnContainers.Core.Models.Token
{
    public class UserToken
    {
        [JsonProperty("id_token")]
        public string IdToken { get; set; }

		[JsonProperty("access_token")]
        public string AccessToken { get; set; }

		[JsonProperty("expires_in")]
        private int ExpiresInRaw { get; set; }


        [JsonIgnore]
        public System.TimeSpan ExpiresIn => System.TimeSpan.FromSeconds(this.ExpiresInRaw);

		[JsonProperty("token_type")]
        public string TokenType { get; set; }

		[JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}
