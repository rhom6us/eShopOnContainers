using System.Collections.Generic;
using Identity.API.Data;
using JetBrains.Annotations;

namespace Identity.API.IdentityServerExternalAuth
{
    public interface IProviderRepository
    {
        [NotNull, ItemNotNull]
        IEnumerable<LoginProvider> Get();

        [CanBeNull]
        LoginProvider Find([NotNull] string providerId);

    }
}