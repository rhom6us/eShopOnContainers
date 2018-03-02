using System.Collections.Generic;
using System.Linq;
using Identity.API.Configuration;
using Identity.API.Data;
using JetBrains.Annotations;

namespace Identity.API.IdentityServerExternalAuth {
    public class ProviderRepository : IProviderRepository {
        private readonly LoginProvider[] _db = Config.GetProviders().ToArray();
        public ProviderRepository() {
        }

        [CanBeNull]
        public LoginProvider Find(string providerId) => _db.SingleOrDefault(p => p.Id == providerId);
        [NotNull]
        public IEnumerable<LoginProvider> Get() {
            return _db;
        }
    }
}
