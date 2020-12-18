using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Routing.Localization
{
    internal class LocalizedRoutingDynamicRouteValueResolver : ILocalizedRoutingDynamicRouteValueResolver
    {
        private readonly ILocalizedRoutingProvider _localizedRoutingProvider;

        public LocalizedRoutingDynamicRouteValueResolver(ILocalizedRoutingProvider localizedRoutingProvider)
        {
            _localizedRoutingProvider = localizedRoutingProvider;
        }
        public async Task<RouteValueDictionary> ResolveAsync(RouteValueDictionary values)
        {
            if (!values.ContainsKey("culture") || !values.ContainsKey("controller") || !values.ContainsKey("action")) return values;

            var culture = (string)values["culture"];
            var routeInformationMetadata = await _localizedRoutingProvider.ProvideRouteAsync(culture, (string)values["controller"], (string)values["action"], LocalizationDirection.TranslatedToOriginal);
          
            values["controller"] = routeInformationMetadata.Controller;
            values["action"] = routeInformationMetadata.Action;

            return values;
        }
    }
}
