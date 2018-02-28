using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using IdentityServerExternalAuth.Entities;
using IdentityServerExternalAuth.Helpers;
using IdentityServerExternalAuth.Interfaces;
using IdentityServerExternalAuth.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;

namespace IdentityServerExternalAuth.Providers
{
    public class FacebookAuthProvider<TUser> : IFacebookAuthProvider where TUser : IdentityUser, new()
    {

        private readonly IProviderRepository _providerRepository;
        private readonly HttpClient _httpClient;
        public FacebookAuthProvider(
            IProviderRepository providerRepository,
            HttpClient httpClient
        )
        {
            _providerRepository = providerRepository;
            _httpClient = httpClient;
        }

        public Provider Provider => _providerRepository.Get()
            .FirstOrDefault(x => string.Equals(x.Name, ProviderType.Facebook.ToString(), StringComparison.CurrentCultureIgnoreCase));

        public JObject GetUserInfo(string accessToken)
        {
            if (this.Provider == null)
            {
                throw new ArgumentNullException(nameof(this.Provider));
            }

            var request = new Dictionary<string, string> {
                ["fields"] = "id,email,name,gender,birthday",
                ["access_token"] = accessToken
            };


            var result = _httpClient.GetAsync(this.Provider.UserInfoEndPoint + QueryBuilder.GetQuery(request, ProviderType.Facebook)).Result;
            if (result.IsSuccessStatusCode)
            {
                var infoObject = JObject.Parse(result.Content.ReadAsStringAsync().Result);
                return infoObject;
            }
            return null;
        }
    }
}