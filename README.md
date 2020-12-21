# AspNetCore.Mvc.Routing.Localization
![Build](https://github.com/tomasjurasek/AspNetCore.Mvc.Routing.Localization/workflows/Build/badge.svg)
![Nuget](https://img.shields.io/nuget/v/AspNetCore.Mvc.Routing.Localization)

> WARNING: We support only the **RouteDataRequestCultureProvider** with the RouteDataStringKey = "culture".

The supported combinations of attributes for creating a translated route.

| Status   |      Controller      | Action |
|:--------:|:--------------------:|:------:|
| Supported | LocalizedRouteAttribute | LocalizedRouteAttribute |
| Supported | LocalizedRouteAttribute | RouteAttribute |
| Supported | LocalizedRouteAttribute | Action |
| Not Supported | RouteAttribute | LocalizedRouteAttribute |
| Supported | RouteAttribute | RouteAttribute |
| Supported | RouteAttribute | Action |
| Not Supported | Controller | LocalizedRouteAttribute |
| Supported | Controller | RouteAttribute |
| Supported | Controller | Action |

## Setup
Register the services into the `IServiceCollection`.
```csharp
 services.AddLocalizedRouting();
```
Implement and register the `DynamicRouteValueTransformer`.
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
Register your `DynamicRouteValueTransformer` in the endpoints middleware with the following template.
```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapDynamicControllerRoute<LocalizedRoutingTranslationTransformer>("{culture=en-US}/{controller=Home}/{action=Index}/{id?}");
    endpoints.MapControllerRoute(name: "default", pattern: "{culture=en-US}/{controller=Home}/{action=Index}/{id?}");
});
```
Set up the localization middleware.
```csharp
var supportedCultures = new[]
{
    new CultureInfo("cs-CZ"),
    new CultureInfo("en-US"),
};

var options = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

options.RequestCultureProviders.Clear();
options.RequestCultureProviders.Insert(0, new RouteDataRequestCultureProvider() { RouteDataStringKey = "culture" });

app.UseRequestLocalization(options);
```

Register the TagHelper from the `AspNetCore.Mvc.Routing.Localization` package in the `_ViewImports.cshtml` file which offers to you localize your route in Views.
```csharp
@addTagHelper *, AspNetCore.Mvc.Routing.Localization
```

## Usage

* `LocalizedRouteAttribute` - the attribute defines the localized route. Example `[LocalizedRoute("en-US", "Home")`.
* `<localized-route></localized-route>` - the taghelper which localizes your links defined by the `LocalizedRouteAttribute` attributes. Example `<localized-route asp-controller="Home" asp-action="Index">Home</localized-route>`.

