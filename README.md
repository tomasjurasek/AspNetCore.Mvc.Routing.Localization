# AspNetCore.Mvc.Routing.Localization
![Build](https://github.com/tomasjurasek/AspNetCore.Mvc.Routing.Localization/workflows/Build/badge.svg)
![Nuget](https://img.shields.io/nuget/v/AspNetCore.Mvc.Routing.Localization)

> WARNING: We support just **RouteDataRequestCultureProvider** with the RouteDataStringKey = "culture".

## Setup
Register the services into the `IServiceCollection`.
```csharp
 services.AddLocalizedRouting();
```
Implement nad register the DynamicRouteValueTransformer.
```csharp
public class LocalizedRoutingTranslationTransformer : DynamicRouteValueTransformer
{
    private ILocalizedRoutingDynamicRouteValueResolver _localizedRoutingDynamicRouteValueResolver;
    public LocalizedRoutingTranslationTransformer(ILocalizedRoutingDynamicRouteValueResolver localizedRoutingDynamicRouteValueResolver)
    {
        _localizedRoutingDynamicRouteValueResolver = localizedRoutingDynamicRouteValueResolver;
    }
    public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
    {
        return await _localizedRoutingDynamicRouteValueResolver.ResolveAsync(values);
    }
}
```
```csharp
services.AddSingleton<LocalizedRoutingTranslationTransformer>();
```

Use your DynamicRouteValueTransformer in the endpoints with the following settings.
```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapDynamicControllerRoute<LocalizedRoutingTranslationTransformer>("{culture=en-US}/{controller=Home}/{action=Jedno}/{id?}");
    endpoints.MapControllerRoute(name: "default", pattern: "{culture=en-US}/{controller=Home}/{action=Jedno}/{id?}");
});
```

Register the TagHelper from the `AspNetCore.Mvc.Routing.Localization` package in the `_ViewImports.cshtml` file which offers to you localize your route in Views.
```csharp
@addTagHelper *, AspNetCore.Mvc.Routing.Localization
```

## Using

Use the `LocalizedRoute` attribute for localize your Route.
In the View part you can use the TagHelper `<localized-route asp-controller="Home" asp-action="Index" ...></localized-route>` which localize your links by the`LocalizedRoute` attributes.
