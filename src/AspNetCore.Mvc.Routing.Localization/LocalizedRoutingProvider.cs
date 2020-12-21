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
    /// <summary>
    /// This class loads and provides localized routes. 
    /// You can inherit it and override the source of localized routes.
    /// </summary>
    public class LocalizedRouteProvider : ILocalizedRoutingProvider
    {
        private IEnumerable<LocalizedRoute> _routes = new List<LocalizedRoute>();
        private IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public LocalizedRouteProvider(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        }

        /// <summary>
        /// This method provides route - depends on direction.
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="controler"></param>
        /// <param name="action"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
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

        /// <summary>
        /// This method creates localized routes. 
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
            
                RouteController - LocalizedRouteAction - not yet
                RouteController - RouteAction
                RouteController - OriginalAction

                OriginalController - LocalizedRouteAction - not yet
                OriginalController - RouteAction
                OriginalController - OriginalAction    

             */

            var localizedRoutes = new List<LocalizedRoute>();
            var routeDescriptors = _actionDescriptorCollectionProvider.ActionDescriptors.Items.Select(s => s as ControllerActionDescriptor);

            foreach (var routeDescriptor in routeDescriptors)
            {
                routeDescriptor.RouteValues.TryGetValue("controller", out var controller);
                routeDescriptor.RouteValues.TryGetValue("action", out var action);

                //Always add default name
                AddLocalizedRoute(null, controller, action);

                var controllerLocalizedRouteAttributes = GetControllersAttribute<LocalizedRouteAttribute>(routeDescriptor)
                    .Distinct(); //Implement IEqualityComparer

                // Can be optimalized by the controllerLocalizedRouteAttributes.Any
                var actionLocalizedRouteAttributes = GetMethodsAttribute<LocalizedRouteAttribute>(routeDescriptor)
                    .Distinct();//Implement IEqualityComparer

                // Can be optimalized by the actionLocalizedRouteAttributes.Any
                var actionRouteAttribute = GetMethodsAttribute<RouteAttribute>(routeDescriptor)
                     .FirstOrDefault();

                //Combinations RouteController or OriginalController
                if (!controllerLocalizedRouteAttributes.Any())
                {
                    var controllerRouteAttribute = GetControllersAttribute<RouteAttribute>(routeDescriptor)
                        .FirstOrDefault();

                    if (controllerRouteAttribute != null || actionRouteAttribute != null)
                    {
                        AddLocalizedRoute(null, controllerRouteAttribute?.Template ?? controller, actionRouteAttribute?.Template ?? action);
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
                    else if (actionRouteAttribute != null) // RouteAttribute
                    {
                        AddLocalizedRoute(controllerLocalizedRouteAttribute.Culture, controllerLocalizedRouteAttribute.Template, actionRouteAttribute.Template);
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



