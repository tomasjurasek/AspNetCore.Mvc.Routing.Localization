using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Routing.Localization
{
    public class LocalizedRoutingDynamicRouteValueResolver : ILocalizedRoutingDynamicRouteValueResolver
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
            var route = await _localizedRoutingProvider.ProvideRouteAsync(culture, (string)values["controller"], (string)values["action"], ProvideRouteType.TranslatedToOriginal);
            var routeValues = route.Split('/');
          
            values["controller"] = routeValues[0];
            values["action"] = routeValues[1];

            return values;
        }
    }
}
