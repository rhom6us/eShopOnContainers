using System;

namespace Microsoft.Extensions.DependencyInjection {
    internal static class IdentityServer4StartupExtensions {

        public static IServiceCollection AddSuppresedChain(this IServiceCollection services, Action<IServiceCollection> nestedAction)
        {
            nestedAction(services);
            return services;
        }

        public static IServiceCollection AddSuppresedChain<TBuilder>(this IServiceCollection services, Func<IServiceCollection, TBuilder> nestedChainBuilder)
            => IdentityServer4StartupExtensions.AddSuppresedChain<TBuilder>(services, nestedChainBuilder, p => { });

        public static IServiceCollection AddSuppresedChain<TBuilder>(this IServiceCollection services, Func<IServiceCollection, TBuilder> nestedChainBuilder, Action<TBuilder> builderAction)
        {
            var resultBuilder = nestedChainBuilder(services);
            builderAction(resultBuilder);
            return services;

        }
    }
}