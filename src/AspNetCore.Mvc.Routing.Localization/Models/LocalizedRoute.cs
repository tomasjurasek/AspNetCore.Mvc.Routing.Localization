using System.Collections.Generic;

namespace AspNetCore.Mvc.Routing.Localization.Models
{
    public sealed class LocalizedRoute
    {
        public string Culture { get; set; }

        public RouteInformation Original { get; set; }

        public RouteInformation Translated { get; set; }
    }

    internal class LocalizedRouteComparer : IEqualityComparer<LocalizedRoute>
    {
        public bool Equals(LocalizedRoute x, LocalizedRoute y)
        {
            return x.Culture == y.Culture &&
                x.Original == y.Original &&
                x.Translated == y.Translated;
        }

        public int GetHashCode(LocalizedRoute obj)
        {
            return obj.Culture?.GetHashCode() ?? 0 +
                obj.Original.GetHashCode() +
                obj.Translated.GetHashCode();
        }
    }
}
