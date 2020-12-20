﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System;

namespace AspNetCore.Mvc.Routing.Localization.Filters
{
    /// <summary>
    /// This filter validates supported culture 
    /// </summary>
    public class SupportedCultureValidatorActionFilter : ActionFilterAttribute
    {
        public IOptions<RequestLocalizationOptions> Options { get; }

        public SupportedCultureValidatorActionFilter(IOptions<RequestLocalizationOptions> options)
        {
            Options = options;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var culture = context.RouteData.Values["culture"] as string;
            if (string.IsNullOrEmpty(culture) || !Options.Value.SupportedCultures.Contains(new System.Globalization.CultureInfo(culture)))
            {
                throw new ArgumentException("The request does not contain a culture");
            }

            base.OnActionExecuting(context);
        }
    }
}
