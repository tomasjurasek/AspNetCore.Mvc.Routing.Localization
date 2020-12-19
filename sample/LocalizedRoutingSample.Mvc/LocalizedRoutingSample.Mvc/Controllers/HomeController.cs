using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LocalizedRoutingSample.Mvc.Models;
using AspNetCore.Mvc.Routing.Localization.Attributes;

namespace LocalizedRoutingSample.Mvc.Controllers
{
    [LocalizedRoute("en-US", "Home")]
    public class HomeController : Controller
    {

        [LocalizedRoute("en-US", "Index")]
        public IActionResult Index()
        {
            return View();
        }

        [LocalizedRoute("en-US", "Privacy")]
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
