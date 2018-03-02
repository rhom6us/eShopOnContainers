using System.Collections.Generic;
using Identity.API.Data;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace Identity.API.IdentityServerExternalAuth.Providers {
    public abstract class ExternalAuthProvider : IExternalAuthProvider {
        private readonly IProviderRepository _providerRepository;
        protected ExternalAuthProvider(IProviderRepository providerRepository) {
            _providerRepository = providerRepository;
        }

        [CanBeNull]
        protected abstract string ProviderKey { get; }

        string IExternalAuthProvider.ProviderKey => this.ProviderKey;


        [CanBeNull]
        protected LoginProvider LoginProvider => _providerRepository.Find(this.ProviderKey);

        [CanBeNull]
        public abstract JObject GetUserInfo(string accessToken);

        protected abstract string BuildQuery(Dictionary<string, string> values);

    }
}