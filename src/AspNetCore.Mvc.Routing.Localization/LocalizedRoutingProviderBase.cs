using AspNetCore.Mvc.Routing.Localization.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Routing.Localization
{
    public abstract class LocalizedRoutingProviderBase
    {
        protected abstract Task<IEnumerable<RouteInformation>> GetRoutesAsync();
    }
}
