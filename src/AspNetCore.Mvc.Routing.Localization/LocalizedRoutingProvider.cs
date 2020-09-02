using AspNetCore.Mvc.Routing.Localization.Attributes;
using AspNetCore.Mvc.Routing.Localization.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Routing.Localization
{
    internal class LocalizedRouteProvider : LocalizedRoutingProviderBase, ILocalizedRoutingProvider
    {
        private static IEnumerable<RouteInformation> Routes = new List<RouteInformation>();
        private IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

        public LocalizedRouteProvider(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        }

        public async Task<string> ProvideRouteAsync(string culture, string controler, string action, ProvideRouteType type)
        {
            if (!Routes.Any())
            {
                Routes = await GetRoutesAsync();
            }

            if (type == ProvideRouteType.TranslatedToOriginal)
            {
                return TranslatedToOriginal(culture, controler, action);
            }
            else if (type == ProvideRouteType.OriginalToTranslated)
            {
                return OriginalToTranslated(culture, controler, action);
            }

            return null;
        }

        private string TranslatedToOriginal(string culture, string controller, string action)
        {
            var route = Routes.FirstOrDefault(s => s.Culture == culture && s.Template == $"{controller}/{action}");
            if (route == null)
            {
                route = Routes.FirstOrDefault(s => s.Culture == null && s.Template == $"{controller}/{action}");
            }
            return route.Original;
        }

        private string OriginalToTranslated(string culture, string controller, string action)
        {
            var route = Routes.FirstOrDefault(s => s.Culture == culture && s.Original == $"{controller}/{action}");
            if (route == null)
            {
                route = Routes.FirstOrDefault(s => s.Culture == null && s.Original == $"{controller}/{action}");
            }
            return route.Template;
        }

        protected override Task<IEnumerable<RouteInformation>> GetRoutesAsync()
        {
            var routesInformations = new List<RouteInformation>();

            foreach (var route in _actionDescriptorCollectionProvider.ActionDescriptors.Items)
            {
                if (route is ControllerActionDescriptor)
                {
                    var routeDescriptor = route as ControllerActionDescriptor;
                    route.RouteValues.TryGetValue("controller", out var controller);
                    route.RouteValues.TryGetValue("action", out var action);

                    var controllerCustomLocalizedRouteAttributes = routeDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(LocalizedRouteAttribute), true).Select(s => s as LocalizedRouteAttribute);
                    foreach (var controllerCustomLocalizedRouteAttribute in controllerCustomLocalizedRouteAttributes)
                    {
                        var routeInformation = new RouteInformation();


                        routeInformation.Original += $"{controller}/{action}";
                        routeInformation.Culture = controllerCustomLocalizedRouteAttribute.Culture;

                        var actionCustomLocalizedRouteAttribute = routeDescriptor.MethodInfo.GetCustomAttributes(typeof(LocalizedRouteAttribute), true)
                            .Select(s => s as LocalizedRouteAttribute)
                            .FirstOrDefault(s => s.Culture == controllerCustomLocalizedRouteAttribute.Culture);


                        if (actionCustomLocalizedRouteAttribute != null)
                        {
                            routeInformation.Template += $"{controllerCustomLocalizedRouteAttribute.Template}/{actionCustomLocalizedRouteAttribute.Template}";
                        }
                        else
                        {
                            var actionRouteAttribute = routeDescriptor.MethodInfo.GetCustomAttributes(typeof(RouteAttribute), true)
                               .Select(s => s as RouteAttribute)
                               .FirstOrDefault();
                            if (actionRouteAttribute != null)
                            {
                                routeInformation.Template += $"{controllerCustomLocalizedRouteAttribute.Template}/{actionRouteAttribute.Template}";
                            }
                        }

                        if (string.IsNullOrEmpty(routeInformation.Template))
                        {
                            routeInformation.Template = $"{controller}/{action}";
                        }

                        routesInformations.Add(routeInformation);
                    }

                    var controllerRouteAttribute = routeDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(RouteAttribute), true)
                         .Select(s => s as RouteAttribute)
                         .FirstOrDefault();
                    if (controllerRouteAttribute != null)
                    {
                        var actionCustomLocalizedRouteAttributes = routeDescriptor.MethodInfo.GetCustomAttributes(typeof(LocalizedRouteAttribute), true).Select(s => s as LocalizedRouteAttribute);
                        foreach (var actionCustomLocalizedRouteAttribute in actionCustomLocalizedRouteAttributes)
                        {
                            var routeInformation = new RouteInformation();

                            routeInformation.Original += $"{controller}/{action}";
                            routeInformation.Culture = actionCustomLocalizedRouteAttribute.Culture;
                            routeInformation.Template = $"{controllerRouteAttribute.Template}/{actionCustomLocalizedRouteAttribute.Template}";

                            if (string.IsNullOrEmpty(routeInformation.Template))
                            {
                                routeInformation.Template = $"{controller}/{action}";
                            }

                            routesInformations.Add(routeInformation);
                        }

                        var actionRouteAttribute = routeDescriptor.MethodInfo.GetCustomAttributes(typeof(RouteAttribute), true)
                        .Select(s => s as RouteAttribute)
                        .FirstOrDefault();

                        if (actionRouteAttribute != null)
                        {
                            var routeInformation = new RouteInformation();
                            routeInformation.Original += $"{controller}/{action}";
                            routeInformation.Template = $"{controllerRouteAttribute.Template}/{actionRouteAttribute.Template}";
                            routesInformations.Add(routeInformation);
                        }
                    }
                }
            }

            return Task.FromResult(routesInformations.Distinct(new RouteInformationComparer()));
        }
    }
}



