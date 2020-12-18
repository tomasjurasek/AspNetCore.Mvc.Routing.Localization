using AspNetCore.Mvc.Routing.Localization.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Routing.Localization
{
    public abstract class LocalizedRoutingProviderBase
    {
        protected abstract Task<IEnumerable<LocalizedRoute>> GetRoutesAsync();
    }
}
