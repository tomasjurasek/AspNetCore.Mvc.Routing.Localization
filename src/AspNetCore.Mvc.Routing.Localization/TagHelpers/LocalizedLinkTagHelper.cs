using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Routing.Localization.TagHelpers
{
    public class LocalizedLinkTagHelper : AnchorTagHelper
    {
        private readonly IActionContextAccessor _contextAccessor;
        private readonly ILocalizedRoutingProvider _translatedService;

        public LocalizedLinkTagHelper(IHtmlGenerator generator, IActionContextAccessor contextAccessor, ILocalizedRoutingProvider translatedService) : base(generator)
        {
            _contextAccessor = contextAccessor;
            _translatedService = translatedService;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "a";
            var culture = _contextAccessor.ActionContext.RouteData.Values["culture"]?.ToString();

            var routeInformationMetadata = await _translatedService.ProvideRouteAsync(culture, Controller, Action, ProvideRouteType.OriginalToTranslated);
            Controller = routeInformationMetadata.Controller;
            Action = routeInformationMetadata.Action;

            await base.ProcessAsync(context, output);
        }
    }
}
