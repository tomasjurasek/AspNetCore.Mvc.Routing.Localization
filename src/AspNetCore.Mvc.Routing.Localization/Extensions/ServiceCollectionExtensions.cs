using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AspNetCore.Mvc.Routing.Localization.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLocalizedRouting(this IServiceCollection services)
        {
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.TryAddSingleton<ILocalizedRoutingDynamicRouteValueResolver, LocalizedRoutingDynamicRouteValueResolver>();
            services.TryAddSingleton<ILocalizedRoutingProvider, LocalizedRouteProvider>();
            return services;
        }
    }
}
