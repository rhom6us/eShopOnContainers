using IdentityServerExternalAuth.Entities;

namespace IdentityServerExternalAuth.Interfaces
{
    public interface ITwitterAuthProvider : IExternalAuthProvider
    {
        Provider Provider { get; }
    }
}