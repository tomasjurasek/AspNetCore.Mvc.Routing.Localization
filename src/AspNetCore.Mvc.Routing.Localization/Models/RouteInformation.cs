using System.Collections.Generic;

namespace AspNetCore.Mvc.Routing.Localization.Models
{
    public sealed class RouteInformation
    {
        public string Culture { get; set; }

        public RouteInformationMetadata Original { get; set; }

        public RouteInformationMetadata Translated { get; set; }
    }

    internal class RouteInformationComparer : IEqualityComparer<RouteInformation>
    {
        public bool Equals(RouteInformation x, RouteInformation y)
        {
            return x.Culture == y.Culture &&
                x.Original == y.Original &&
                x.Translated == y.Translated;
        }

        public int GetHashCode(RouteInformation obj)
        {
            return obj.Culture?.GetHashCode() ?? 0 +
                obj.Original.GetHashCode() +
                obj.Translated.GetHashCode();
        }
    }
}
