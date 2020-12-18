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
        private static IEnumerable<LocalizedRoute> Routes = new List<LocalizedRoute>();
        private IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

        public LocalizedRouteProvider(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        }

        public async Task<RouteInformation> ProvideRouteAsync(string culture, string controler, string action, LocalizationDirection direction)
        {
            if (!Routes.Any())
            {
                Routes = await GetRoutesAsync();
            }

            if (direction == LocalizationDirection.TranslatedToOriginal)
            {
                return TranslatedToOriginal(culture, controler, action);
            }
            else if (direction == LocalizationDirection.OriginalToTranslated)
            {
                return OriginalToTranslated(culture, controler, action);
            }

            return null;
        }

        private RouteInformation TranslatedToOriginal(string culture, string controller, string action)
        {
            var routeInformation = Routes.FirstOrDefault(s => s.Culture == culture && s.Translated?.Action == action && s.Translated?.Controller == controller);
            if (routeInformation == null)
            {
                routeInformation = Routes.FirstOrDefault(s => s.Culture == null && s.Translated?.Action == action && s.Translated?.Controller == controller);
            }
            return routeInformation?.Original;
        }

        private RouteInformation OriginalToTranslated(string culture, string controller, string action)
        {
            var routeInformation = Routes.FirstOrDefault(s => s.Culture == culture && s.Original?.Action == action && s.Original?.Controller == controller);
            if (routeInformation == null)
            {
                routeInformation = Routes.FirstOrDefault(s => s.Culture == null && s.Original?.Action == action && s.Original?.Controller == controller);
            }
            return routeInformation?.Translated;
        }

        protected override Task<IEnumerable<LocalizedRoute>> GetRoutesAsync()
        {
            var routesInformations = new List<LocalizedRoute>();

            foreach (var route in _actionDescriptorCollectionProvider.ActionDescriptors.Items)
            {
                if (route is ControllerActionDescriptor)
                {
                    var routeDescriptor = route as ControllerActionDescriptor;
                    route.RouteValues.TryGetValue("controller", out var controller);
                    route.RouteValues.TryGetValue("action", out var action);

                    routesInformations.Add(new LocalizedRoute
                    {
                        Culture = null,
                        Original = new RouteInformation
                        {
                            Controller = controller,
                            Action = action
                        },
                        Translated = new RouteInformation
                        {
                            Controller = controller,
                            Action = action
                        }
                    });

                    var controllerCustomLocalizedRouteAttributes = routeDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(LocalizedRouteAttribute), true).Select(s => s as LocalizedRouteAttribute);
                    foreach (var controllerCustomLocalizedRouteAttribute in controllerCustomLocalizedRouteAttributes)
                    {
                        var routeInformation = new LocalizedRoute();
                        routeInformation.Original = new RouteInformation
                        {
                            Controller = controller,
                            Action = action
                        };
                        routeInformation.Culture = controllerCustomLocalizedRouteAttribute.Culture;

                        var actionCustomLocalizedRouteAttribute = routeDescriptor.MethodInfo.GetCustomAttributes(typeof(LocalizedRouteAttribute), true)
                            .Select(s => s as LocalizedRouteAttribute)
                            .FirstOrDefault(s => s.Culture == controllerCustomLocalizedRouteAttribute.Culture);


                        if (actionCustomLocalizedRouteAttribute != null)
                        {
                            routeInformation.Translated = new RouteInformation
                            {
                                Controller = controllerCustomLocalizedRouteAttribute.Template,
                                Action = actionCustomLocalizedRouteAttribute.Template
                            };
                        }
                        else
                        {
                            var actionRouteAttribute = routeDescriptor.MethodInfo.GetCustomAttributes(typeof(RouteAttribute), true)
                               .Select(s => s as RouteAttribute)
                               .FirstOrDefault();
                            if (actionRouteAttribute != null)
                            {
                                routeInformation.Translated = new RouteInformation
                                {
                                    Controller = controllerCustomLocalizedRouteAttribute.Template,
                                    Action = actionRouteAttribute.Template
                                };
                            }
                        }

                        if (routeInformation.Translated == null)
                        {
                            routeInformation.Translated = new RouteInformation
                            {
                                Controller = controller,
                                Action = action
                            };
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
                            var routeInformation = new LocalizedRoute();
                            routeInformation.Culture = actionCustomLocalizedRouteAttribute.Culture;
                            routeInformation.Original = new RouteInformation
                            {
                                Controller = controller,
                                Action = action
                            };
                            routeInformation.Translated = new RouteInformation
                            {
                                Controller = controllerRouteAttribute.Template,
                                Action = actionCustomLocalizedRouteAttribute.Template
                            };

                            if (routeInformation.Translated == null)
                            {
                                routeInformation.Translated = new RouteInformation
                                {
                                    Controller = controller,
                                    Action = action
                                };
                            }

                            routesInformations.Add(routeInformation);
                        }

                        var actionRouteAttribute = routeDescriptor.MethodInfo.GetCustomAttributes(typeof(RouteAttribute), true)
                        .Select(s => s as RouteAttribute)
                        .FirstOrDefault();

                        if (actionRouteAttribute != null)
                        {
                            var routeInformation = new LocalizedRoute();
                            routeInformation.Original = new RouteInformation
                            {
                                Controller = controller,
                                Action = action
                            };
                            routeInformation.Translated = new RouteInformation
                            {
                                Controller = controllerRouteAttribute.Template,
                                Action = actionRouteAttribute.Template
                            };
                            routesInformations.Add(routeInformation);
                        }
                    }
                }
            }

            return Task.FromResult(routesInformations.Distinct(new LocalizedRouteComparer()));
        }
    }
}



