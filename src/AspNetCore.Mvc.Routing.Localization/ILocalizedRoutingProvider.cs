using System.Threading.Tasks;
using static AspNetCore.Mvc.Routing.Localization.LocalizedRouteProvider;

namespace AspNetCore.Mvc.Routing.Localization
{
    public interface ILocalizedRoutingProvider
    {
        Task<string> ProvideRouteAsync(string culture, string controller, string action, ProvideRouteType type);
    }
}