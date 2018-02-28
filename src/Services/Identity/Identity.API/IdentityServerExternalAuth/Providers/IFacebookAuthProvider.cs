using IdentityServerExternalAuth.Entities;

namespace IdentityServerExternalAuth.Interfaces
{
    public interface IFacebookAuthProvider : IExternalAuthProvider
    {
        Provider Provider { get; }
    }
}