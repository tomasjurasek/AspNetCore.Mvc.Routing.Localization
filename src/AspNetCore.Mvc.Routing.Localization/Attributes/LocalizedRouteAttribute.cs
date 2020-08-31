using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Mvc.Routing.Localization.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class LocalizedRouteAttribute : Attribute, ILocalizedRouteTemplateProvider
    {
        public LocalizedRouteAttribute(string culture, string template)
        {
            Culture = culture;
            Template = template;
        }

        public string Culture { get; }

        public string Template { get; }
    }


    public interface ILocalizedRouteTemplateProvider
    {
        string Template { get; }
        string Culture { get; }
    }
}
