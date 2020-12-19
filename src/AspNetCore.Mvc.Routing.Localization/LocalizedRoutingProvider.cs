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
        private IEnumerable<LocalizedRoute> _routes = new List<LocalizedRoute>();
        private IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public LocalizedRouteProvider(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        }

        public async Task<RouteInformation> ProvideRouteAsync(string culture, string controler, string action, LocalizationDirection direction)
        {
            if (!_routes.Any())
            {
                try
                {
                    await _semaphore.WaitAsync();
                    _routes = await GetRoutesAsync();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            return GetLocalizedRoute(culture, controler, action, direction);
        }

        public RouteInformation GetLocalizedRoute(string culture, string controller, string action, LocalizationDirection direction)
        {
            Func<string, LocalizedRoute> translated = (currentCulture) =>
                 _routes
                    .FirstOrDefault(s => s.Culture == currentCulture && s.Translated?.Action == action && s.Translated?.Controller == controller);

            Func<string, LocalizedRoute> original = (currentCulture) =>
                    _routes
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

                    routesInformations.Add(LocalizedRoute.Create(
                        null,
                        RouteInformation.Create(controller, action),
                        RouteInformation.Create(controller, action)));

                    var controllerCustomLocalizedRouteAttributes = GetControllersAttribute<LocalizedRouteAttribute>(routeDescriptor);

                    foreach (var controllerCustomLocalizedRouteAttribute in controllerCustomLocalizedRouteAttributes)
                    {
                        var routeInformation = LocalizedRoute.Create(
                            controllerCustomLocalizedRouteAttribute.Culture,
                            RouteInformation.Create(controller, action),
                            null);

                        var actionCustomLocalizedRouteAttribute = GetMethodsAttribute<LocalizedRouteAttribute>(routeDescriptor)
                            .FirstOrDefault(s => s.Culture == controllerCustomLocalizedRouteAttribute.Culture);

                        if (actionCustomLocalizedRouteAttribute != null)
                        {
                            routeInformation.Translated = RouteInformation.Create(
                                controllerCustomLocalizedRouteAttribute.Template,
                               actionCustomLocalizedRouteAttribute.Template);
                        }
                        else
                        {
                            var actionRouteAttribute = GetMethodsAttribute<RouteAttribute>(routeDescriptor)
                                .FirstOrDefault();

                            if (actionRouteAttribute != null)
                            {
                                routeInformation.Translated = RouteInformation.Create(
                                    controllerCustomLocalizedRouteAttribute.Template,
                                    actionRouteAttribute.Template);
                            }
                        }

                        if (routeInformation.Translated == null)
                        {
                            routeInformation.Translated = RouteInformation.Create(controller, action);
                        }

                        routesInformations.Add(routeInformation);
                    }

                    var controllerRouteAttribute = GetControllersAttribute<RouteAttribute>(routeDescriptor).FirstOrDefault();

                    if (controllerRouteAttribute != null)
                    {
                        IEnumerable<LocalizedRouteAttribute> actionCustomLocalizedRouteAttributes = GetMethodsAttribute<LocalizedRouteAttribute>(routeDescriptor);

                        foreach (var actionCustomLocalizedRouteAttribute in actionCustomLocalizedRouteAttributes)
                        {
                            var routeInformation = LocalizedRoute.Create(
                                actionCustomLocalizedRouteAttribute.Culture,
                                RouteInformation.Create(controller, action),
                                RouteInformation.Create(controllerRouteAttribute.Template, actionCustomLocalizedRouteAttribute.Template));

                            if (routeInformation.Translated == null)
                            {
                                routeInformation.Translated = RouteInformation.Create(controller, action);
                            }

                            routesInformations.Add(routeInformation);
                        }

                        var actionRouteAttribute = GetMethodsAttribute<RouteAttribute>(routeDescriptor)
                            .FirstOrDefault();

                        if (actionRouteAttribute != null)
                        {
                            var routeInformation = LocalizedRoute.Create(
                                null,
                                RouteInformation.Create(controller, action),
                                RouteInformation.Create(controllerRouteAttribute.Template, actionRouteAttribute.Template));

                            routesInformations.Add(routeInformation);
                        }
                    }
                }
            }

            return routesInformations;
        }

        private static IEnumerable<T> GetMethodsAttribute<T>(ControllerActionDescriptor routeDescriptor) where T : class
        {
            return routeDescriptor.MethodInfo
                .GetCustomAttributes(typeof(T), true)
                .Select(s => s as T);
        }

        private static IEnumerable<T> GetControllersAttribute<T>(ControllerActionDescriptor routeDescriptor) where T : class
        {
            return routeDescriptor.ControllerTypeInfo
                .GetCustomAttributes(typeof(T), true)
                .Select(s => s as T);
        }
    }
}



