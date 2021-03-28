using AspNetCore.Mvc.Routing.Localization.Attributes;
using AspNetCore.Mvc.Routing.Localization.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Routing.Localization
{
    /// <summary>
    /// Loads and provides localized routes. 
    /// You can inherit it and override the source of localized routes.
    /// </summary>
    public class LocalizedRouteProvider : ILocalizedRoutingProvider
    {
        private IEnumerable<LocalizedRoute> _routes = Enumerable.Empty<LocalizedRoute>();
        private bool _routesLoaded = false;
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private readonly IControllerActionDescriptorProvider _controllerActionDescriptorProvider;
        private readonly IList<CultureInfo> _supportedCultures;

        public LocalizedRouteProvider(IControllerActionDescriptorProvider controllerActionDescriptorProvider,
            IOptions<RequestLocalizationOptions> localizationOptions)
        {
            _controllerActionDescriptorProvider = controllerActionDescriptorProvider;
            _supportedCultures = localizationOptions.Value.SupportedCultures;
        }

        /// <summary>
        /// Provides a route - depends on the direction.
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="controler"></param>
        /// <param name="action"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public async Task<RouteInformation> ProvideRouteAsync(string culture, string controler, string action, LocalizationDirection direction)
        {
            if (!_routesLoaded && !_routes.Any())
            {
                await _semaphore.WaitAsync();
                try
                {
                    _routes = await GetRoutesAsync();
                    _routesLoaded = true;
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            return GetLocalizedRoute(culture, controler, action, direction);
        }

        /// <summary>
        /// Creates localized routes 
        /// You can override this method to change the source of localized routes - DB, File,...
        /// </summary>
        /// <returns>Localized routes</returns>
        protected virtual async Task<IEnumerable<LocalizedRoute>> GetRoutesAsync()
        {
            /* 
                If there is no LocalizedRouteAction it is for all cultures
                
                LocalizedRouteController -  LocalizedRouteAction
                LocalizedRouteController -  RouteAction
                LocalizedRouteController -  OriginalAction    
            
                RouteController - LocalizedRouteAction 
                RouteController - RouteAction
                RouteController - OriginalAction

                OriginalController - LocalizedRouteAction 
                OriginalController - RouteAction
                OriginalController - OriginalAction    

             */

            var localizedRoutes = new List<LocalizedRoute>();

            foreach (var routeDescriptor in _controllerActionDescriptorProvider.Get())
            {
                routeDescriptor.RouteValues.TryGetValue("controller", out var controller);
                routeDescriptor.RouteValues.TryGetValue("action", out var action);

                var controllerLocalizedRouteAttributes = GetControllersAttribute<LocalizedRouteAttribute>(routeDescriptor)
                    .Distinct(); //Implement IEqualityComparer

                // Can be optimalized by the controllerLocalizedRouteAttributes.Any
                var actionLocalizedRouteAttributes = GetMethodsAttribute<LocalizedRouteAttribute>(routeDescriptor)
                    .Distinct();//Implement IEqualityComparer

                // Can be optimalized by the actionLocalizedRouteAttributes.Any
                var actionRouteAttributes = GetMethodsAttribute<RouteAttribute>(routeDescriptor)
                     .Distinct();//Implement IEqualityComparer

                if (!controllerLocalizedRouteAttributes.Any())
                {
                    var controllerRouteAttributes = GetControllersAttribute<RouteAttribute>(routeDescriptor)
                        .Distinct();

                    if (!controllerRouteAttributes.Any())
                    {
                        if (!actionLocalizedRouteAttributes.Any())
                        {
                            if (!actionRouteAttributes.Any())
                            {
                                foreach (var culture in _supportedCultures)
                                {
                                    AddLocalizedRoute(culture.Name, controller, action);
                                }
                            }
                            else
                            {
                                foreach (var actionRouteAttribute in actionRouteAttributes)
                                {
                                    foreach (var culture in _supportedCultures)
                                    {
                                        AddLocalizedRoute(culture.Name, controller, actionRouteAttribute.Template);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var actionLocalizedRouteAttribute in actionLocalizedRouteAttributes)
                            {
                                AddLocalizedRoute(actionLocalizedRouteAttribute.Culture, controller, actionLocalizedRouteAttribute.Template);
                            }
                        }

                    }
                    else
                    {
                        foreach (var controllerRouteAttribute in controllerRouteAttributes)
                        {
                            if (!actionLocalizedRouteAttributes.Any())
                            {
                                if (!actionRouteAttributes.Any())
                                {
                                    foreach (var culture in _supportedCultures)
                                    {
                                        AddLocalizedRoute(culture.Name, controllerRouteAttribute.Template, action);
                                    }
                                }
                                else
                                {
                                    foreach (var actionRouteAttribute in actionRouteAttributes)
                                    {
                                        foreach (var culture in _supportedCultures)
                                        {
                                            AddLocalizedRoute(culture.Name, controllerRouteAttribute.Template, actionRouteAttribute.Template);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (var actionLocalizedRouteAttribute in actionLocalizedRouteAttributes)
                                {
                                    AddLocalizedRoute(actionLocalizedRouteAttribute.Culture, controllerRouteAttribute.Template, actionLocalizedRouteAttribute.Template);
                                }
                            }
                        }
                    }
                }

                foreach (var controllerLocalizedRouteAttribute in controllerLocalizedRouteAttributes)
                {
                    var actionLocalizedRouteAttribute = actionLocalizedRouteAttributes
                       .FirstOrDefault(s => s.Culture == controllerLocalizedRouteAttribute.Culture);

                    if (actionLocalizedRouteAttribute != null) // LocalizedRouteAttribute
                    {
                        AddLocalizedRoute(controllerLocalizedRouteAttribute.Culture, controllerLocalizedRouteAttribute.Template, actionLocalizedRouteAttribute.Template);
                    }
                    else if (actionRouteAttributes.Any()) // RouteAttribute
                    {
                        foreach (var actionRouteAttribute in actionRouteAttributes)
                        {
                            AddLocalizedRoute(controllerLocalizedRouteAttribute.Culture, controllerLocalizedRouteAttribute.Template, actionRouteAttribute.Template);
                        }
                    }
                    else // Original
                    {
                        AddLocalizedRoute(controllerLocalizedRouteAttribute.Culture, controllerLocalizedRouteAttribute.Template, action);
                    }
                }

                void AddLocalizedRoute(string cultureName, string controllerName, string actionName)
                {
                    localizedRoutes.Add(LocalizedRoute.Create(cultureName,
                                               RouteInformation.Create(controller, action),
                                               RouteInformation.Create(controllerName, actionName)));
                }
            }

            return localizedRoutes;
        }

        private RouteInformation GetLocalizedRoute(string culture, string controller, string action, LocalizationDirection direction)
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



