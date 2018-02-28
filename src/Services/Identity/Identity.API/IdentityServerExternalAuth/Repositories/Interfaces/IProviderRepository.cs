using System.Collections.Generic;
using IdentityServerExternalAuth.Entities;

namespace IdentityServerExternalAuth.Repositories.Interfaces
{
    public interface IProviderRepository
    {
        IEnumerable<Provider> Get();


    }
}