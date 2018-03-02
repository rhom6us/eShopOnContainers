using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Identity.API.Data;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace Identity.API.IdentityServerExternalAuth.Providers
{
    public class TwitterAuthProvider : ExternalAuthProvider {
        private readonly HttpClient _httpClient;
        public TwitterAuthProvider(
            IProviderRepository providerRepository,
            HttpClient httpClient
        ) : base(providerRepository)
        {
            _httpClient = httpClient;
        }

        [NotNull]
        protected override string ProviderKey => "twitter";

        public override JObject GetUserInfo(string accessToken)
        {
            if (this.LoginProvider == null)
            {
                throw new ArgumentNullException(nameof(this.LoginProvider));
            }

            var request = new Dictionary<string, string> {
                {"tokenString", accessToken},
                { "endpoint", this.LoginProvider.UserInfoEndPoint}
            };

            var authorizationHeaderParams = this.BuildQuery(request);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            _httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeaderParams);

            var result = _httpClient.GetAsync(this.LoginProvider.UserInfoEndPoint).Result;

            if (!result.IsSuccessStatusCode)
                return null;

            var infoObject = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            return infoObject;
        }

        protected override string BuildQuery(Dictionary<string, string> values) {
            var token = values["tokenString"];
            var userInfoEndpoint = values["endpoint"];

            var tokenString = token.Split('&').ToDictionary(x => x.Split('=')[0], x => x.Split('=')[1]);
            if (tokenString.Count < 4) return null;

            var oauth_consumer_key = tokenString["oauth_consumer_key"];
            var consumerSecret = tokenString["oauth_consumer_secret"];
            var oauth_token_secret = tokenString["oauth_token_secret"];
            var oauth_token = tokenString["oauth_token"];
            var oauth_version = "1.0";
            var oauth_signature_method = "HMAC-SHA1";
            var oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));

            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var oauth_timestamp = Convert.ToInt64(ts.TotalSeconds).ToString();

            var sd = new SortedDictionary<string, string> {
                        {"oauth_version", oauth_version},
                        {"oauth_consumer_key", oauth_consumer_key},
                        {"oauth_nonce", oauth_nonce},
                        {"oauth_signature_method", oauth_signature_method},
                        {"oauth_timestamp", oauth_timestamp},
                        {"oauth_token", oauth_token}
                    };


            //GS - Build the signature string
            var baseString = string.Empty;
            baseString += "GET" + "&";
            baseString += Uri.EscapeDataString(userInfoEndpoint) + "&";
            foreach (var entry in sd)
                baseString += Uri.EscapeDataString(entry.Key + "=" + entry.Value + "&");

            baseString = baseString.Substring(0, baseString.Length - 3);

            var signingKey = Uri.EscapeDataString(consumerSecret) + "&" + Uri.EscapeDataString(oauth_token_secret);

            var hasher = new HMACSHA1(new ASCIIEncoding().GetBytes(signingKey));

            var signatureString = Convert.ToBase64String(hasher.ComputeHash(new ASCIIEncoding().GetBytes(baseString)));

            //prepare the request
            var authorizationHeaderParams = string.Empty;
            authorizationHeaderParams += "OAuth ";
            authorizationHeaderParams += "oauth_nonce=" + "\"" + Uri.EscapeDataString(oauth_nonce) + "\",";

            authorizationHeaderParams += "oauth_signature_method=" + "\"" + Uri.EscapeDataString(oauth_signature_method) + "\",";

            authorizationHeaderParams += "oauth_timestamp=" + "\"" + Uri.EscapeDataString(oauth_timestamp) + "\",";

            authorizationHeaderParams += "oauth_consumer_key=" + "\"" + Uri.EscapeDataString(oauth_consumer_key) + "\",";

            authorizationHeaderParams += "oauth_token=" + "\"" + Uri.EscapeDataString(oauth_token) + "\",";

            authorizationHeaderParams += "oauth_signature=" + "\"" + Uri.EscapeDataString(signatureString) + "\",";

            authorizationHeaderParams += "oauth_version=" + "\"" + Uri.EscapeDataString(oauth_version) + "\"";

            return authorizationHeaderParams;
        }

    }
}