using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace Identity.API.IdentityServerExternalAuth.Providers {
    public class FacebookAuthProvider : ExternalAuthProvider {
        [NotNull]
        protected override string ProviderKey => "facebook";

        public FacebookAuthProvider(
            IProviderRepository providerRepository,
            HttpClient httpClient
        ) : base(providerRepository) {
            _httpClient = httpClient;
        }


        public override JObject GetUserInfo(string accessToken) {
            if (this.LoginProvider == null)
                throw new ArgumentNullException(nameof(this.LoginProvider));

            var request = new Dictionary<string, string> {
                ["fields"] = "id,email,name,gender,birthday",
                ["access_token"] = accessToken
            };


            var result = _httpClient.GetAsync(this.LoginProvider.UserInfoEndPoint + BuildQuery(request)).Result;
            if (result.IsSuccessStatusCode) {
                var infoObject = JObject.Parse(result.Content.ReadAsStringAsync().Result);
                return infoObject;
            }

            return null;
        }

        [NotNull]
        protected override string BuildQuery(Dictionary<string, string> values) {
            return $"?fields={values["fields"]}&access_token={values["access_token"]}";
        }

        private readonly HttpClient _httpClient;
    }
}
