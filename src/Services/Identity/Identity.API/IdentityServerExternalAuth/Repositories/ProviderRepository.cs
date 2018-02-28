using System.Collections.Generic;
using IdentityServerExternalAuth.Entities;
using IdentityServerExternalAuth.Helpers;
using IdentityServerExternalAuth.Repositories.Interfaces;

namespace IdentityServerExternalAuth.Repositories {
    public class ProviderRepository : IProviderRepository {
        public IEnumerable<Provider> Get() {
            return ProviderDataSource.GetProviders();
        }
    }
}
