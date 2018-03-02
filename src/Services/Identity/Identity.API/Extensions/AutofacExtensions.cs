using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.API.Extensions {
    internal static class AutofacExtensions {
        [NotNull]
        public static AutofacServiceProvider BuildAutofacServiceProvider(this IServiceCollection services) {
            return new AutofacServiceProvider(services.CreateContainerBuilder().Build());
        }

        [NotNull]
        public static ContainerBuilder CreateContainerBuilder(this IServiceCollection services) {
            var container = new ContainerBuilder();
            container.Populate(services);
            return container;
        }
    }
}