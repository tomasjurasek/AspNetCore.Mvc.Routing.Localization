using AspNetCore.Mvc.Routing.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace LocalizedRoutingSample.Mvc
{
    public class LocalizedRoutingTranslationTransformer : DynamicRouteValueTransformer
    {
        private ILocalizedRoutingDynamicRouteValueResolver _localizedRoutingDynamicRouteValueResolver;
        public LocalizedRoutingTranslationTransformer(ILocalizedRoutingDynamicRouteValueResolver localizedRoutingDynamicRouteValueResolver)
        {
            _localizedRoutingDynamicRouteValueResolver = localizedRoutingDynamicRouteValueResolver;
        }
        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            return await _localizedRoutingDynamicRouteValueResolver.ResolveAsync(values);
        }
    }
}
