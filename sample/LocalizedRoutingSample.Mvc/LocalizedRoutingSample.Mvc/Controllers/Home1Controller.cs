using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Mvc.Routing.Localization.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace LocalizedRoutingSample.Mvc.Controllers
{
    [Route("Home1")]
    public class Home1Controller : Controller
    {
        [LocalizedRoute("en-US", "Index")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("Index2")]
        public IActionResult Index2()
        {
            return View();
        }
    }
}
