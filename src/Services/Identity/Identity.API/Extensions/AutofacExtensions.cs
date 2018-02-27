using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.API {
    internal static class AutofacExtensions {
        public static IServiceProvider BuildAutofacServiceProvider(this IServiceCollection services) {
            return new AutofacServiceProvider(services.CreateContainerBuilder().Build());
        }

        public static ContainerBuilder CreateContainerBuilder(this IServiceCollection services) {
            var container = new ContainerBuilder();
            container.Populate(services);
            return container;
        }
    }
}