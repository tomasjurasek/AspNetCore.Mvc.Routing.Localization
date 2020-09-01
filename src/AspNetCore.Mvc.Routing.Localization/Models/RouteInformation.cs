using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Mvc.Routing.Localization.Models
{
    public class RouteInformation
    {
        public string Original { get; set; }
        public string Culture { get; set; }
        public string Template { get; set; }
    }

    public class RouteInformationComparer : IEqualityComparer<RouteInformation>
    {
        public bool Equals(RouteInformation x, RouteInformation y)
        {
            return x.Culture == y.Culture &&
                x.Original == y.Original &&
                x.Template == y.Template;
        }

        public int GetHashCode(RouteInformation obj)
        {
            return obj.Culture?.GetHashCode() ?? 0 +
                obj.Original.GetHashCode() +
                obj.Template.GetHashCode();
        }
    }
}
