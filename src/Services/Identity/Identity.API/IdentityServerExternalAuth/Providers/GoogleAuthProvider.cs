using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace Identity.API.IdentityServerExternalAuth.Providers
{
    public class GoogleAuthProvider : ExternalAuthProvider
    {
        public GoogleAuthProvider(IProviderRepository providerRepository) : base(providerRepository) { }
        public override JObject GetUserInfo(string accessToken) {
            throw new NotImplementedException();
        }

        [NotNull]
        protected override string BuildQuery(Dictionary<string, string> values) {
            return $"?access_token={values["token"]}";
        }

        [NotNull]
        protected override string ProviderKey => "google";
    }
}
