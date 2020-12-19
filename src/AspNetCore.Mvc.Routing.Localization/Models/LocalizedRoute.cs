namespace AspNetCore.Mvc.Routing.Localization.Models
{
    public sealed class LocalizedRoute
    {
        public string Culture { get; set; }

        public RouteInformation Original { get; set; }

        public RouteInformation Translated { get; set; }
    }
}
