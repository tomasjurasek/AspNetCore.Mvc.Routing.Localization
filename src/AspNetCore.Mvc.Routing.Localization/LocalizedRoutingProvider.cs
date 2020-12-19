using AspNetCore.Mvc.Routing.Localization.Attributes;
using AspNetCore.Mvc.Routing.Localization.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Routing.Localization
{
    internal class LocalizedRouteProvider : LocalizedRoutingProviderBase, ILocalizedRoutingProvider
    {
        private IEnumerable<LocalizedRoute> Routes = new List<LocalizedRoute>();
        private IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public LocalizedRouteProvider(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        }

        public async Task<RouteInformation> ProvideRouteAsync(string culture, string controler, string action, LocalizationDirection direction)
        {
            if (!Routes.Any())
            {
                try
                {
                    await semaphore.WaitAsync();
                    Routes = await GetRoutesAsync();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    semaphore.Release();
                }
            }

            return GetLocalizedRoute(culture, controler, action, direction);
        }

        public RouteInformation GetLocalizedRoute(string culture, string controller, string action, LocalizationDirection direction)
        {
            Func<string, LocalizedRoute> translated = (currentCulture) =>
                 Routes
                    .FirstOrDefault(s => s.Culture == currentCulture && s.Translated?.Action == action && s.Translated?.Controller == controller);

            Func<string, LocalizedRoute> original = (currentCulture) =>
                    Routes
                    .FirstOrDefault(s => s.Culture == currentCulture && s.Original?.Action == action && s.Original?.Controller == controller);

            return direction == LocalizationDirection.TranslatedToOriginal
                        ? (translated(culture) ?? translated(null))?.Original
                        : (original(culture) ?? original(null))?.Translated;
        }

        protected override async Task<IList<LocalizedRoute>> GetRoutesAsync()
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
                        var routeInformation = new LocalizedRoute
                        {
                            Culture = controllerCustomLocalizedRouteAttribute.Culture,
                            Original = new RouteInformation
                            {
                                Controller = controller,
                                Action = action
                            }
                        };

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
                            var routeInformation = new LocalizedRoute
                            {
                                Culture = actionCustomLocalizedRouteAttribute.Culture,
                                Original = new RouteInformation
                                {
                                    Controller = controller,
                                    Action = action
                                },
                                Translated = new RouteInformation
                                {
                                    Controller = controllerRouteAttribute.Template,
                                    Action = actionCustomLocalizedRouteAttribute.Template
                                }
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
                            var routeInformation = new LocalizedRoute
                            {
                                Original = new RouteInformation
                                {
                                    Controller = controller,
                                    Action = action
                                },
                                Translated = new RouteInformation
                                {
                                    Controller = controllerRouteAttribute.Template,
                                    Action = actionRouteAttribute.Template
                                }
                            };

                            routesInformations.Add(routeInformation);
                        }
                    }
                }
            }

            return routesInformations;
        }
    }
}



