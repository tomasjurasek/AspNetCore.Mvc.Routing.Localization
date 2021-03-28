using AspNetCore.Mvc.Routing.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LocalizedRoutingSample.Mvc.Controllers
{
    public class LanguagesController : Controller
    {
        private readonly ILocalizedRoutingProvider _localizedRoutingProvider;

        public LanguagesController(ILocalizedRoutingProvider localizedRoutingProvider)
        {
            _localizedRoutingProvider = localizedRoutingProvider;
        }

        public async Task<IActionResult> ChangeLanguage(string language)
        {
            var routeInformation = await _localizedRoutingProvider.ProvideRouteAsync(language, "Home", "Index", LocalizationDirection.OriginalToTranslated);
            return RedirectToRoute(new
            {
                controller = routeInformation.Controller,
                action = routeInformation.Action,
                culture = language
            });
        }
    }
}
