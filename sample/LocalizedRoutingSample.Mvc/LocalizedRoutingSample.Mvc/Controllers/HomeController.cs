using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LocalizedRoutingSample.Mvc.Models;
using AspNetCore.Mvc.Routing.Localization.Attributes;
using AspNetCore.Mvc.Routing.Localization;
using System.Threading.Tasks;

namespace LocalizedRoutingSample.Mvc.Controllers
{
    [LocalizedRoute("en-US", "Home")]
    [LocalizedRoute("cs-CZ", "Domu")]
    public class HomeController : Controller
    {
        private readonly ILocalizedRoutingProvider _localizedRoutingProvider;

        public HomeController(ILocalizedRoutingProvider localizedRoutingProvider)
        {
            _localizedRoutingProvider = localizedRoutingProvider;
        }
        [LocalizedRoute("en-US", "Index")]
        [LocalizedRoute("cs-CZ", "Uvod")]
        public IActionResult Index()
        {
            return View();
        }

        [LocalizedRoute("en-US", "Privacy")]
        [LocalizedRoute("cs-CZ", "Soukromi")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [LocalizedRoute("en-US", "Error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
