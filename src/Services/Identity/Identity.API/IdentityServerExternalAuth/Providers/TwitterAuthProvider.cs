using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using IdentityServerExternalAuth.Entities;
using IdentityServerExternalAuth.Helpers;
using IdentityServerExternalAuth.Interfaces;
using IdentityServerExternalAuth.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;

namespace IdentityServerExternalAuth.Providers
{
    public class TwitterAuthProvider<TUser> : ITwitterAuthProvider where TUser : IdentityUser, new()
    {
        private readonly IProviderRepository _providerRepository;
        private readonly HttpClient _httpClient;
        public TwitterAuthProvider(
            IProviderRepository providerRepository,
            HttpClient httpClient
        )
        {
            _providerRepository = providerRepository;
            _httpClient = httpClient;
        }
        public Provider Provider => _providerRepository.Get()
            .FirstOrDefault(x => x.Name.ToLower() == ProviderType.Twitter.ToString().ToLower());

        public JObject GetUserInfo(string accessToken)
        {
            if (this.Provider == null)
            {
                throw new ArgumentNullException(nameof(this.Provider));
            }

            var request = new Dictionary<string, string>();
            request.Add("tokenString", accessToken);
            request.Add("endpoint", this.Provider.UserInfoEndPoint);

            var authorizationHeaderParams = QueryBuilder.GetQuery(request, ProviderType.Twitter);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            _httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeaderParams);

            var result = _httpClient.GetAsync(this.Provider.UserInfoEndPoint).Result;

            if (result.IsSuccessStatusCode)
            {
                var infoObject = JObject.Parse(result.Content.ReadAsStringAsync().Result);
                return infoObject;
            }
            return null;
        }
    }
}