# AspNetCore.Mvc.Routing.Localization
![Build](https://github.com/tomasjurasek/AspNetCore.Mvc.Routing.Localization/workflows/Build/badge.svg)
![Nuget](https://img.shields.io/nuget/v/AspNetCore.Mvc.Routing.Localization)

## Summary
The package [AspNetCore.Mvc.Routing.Localization](https://www.nuget.org/packages/AspNetCore.Mvc.Routing.Localization) helps you create localized routes. Your routes can be specific per language - `en-US/Products` and `cs-CZ/Produkty`, which are calling the same route.

Supported combinations:

| Status   |      Controller      | Action |
|:--------:|:--------------------:|:------:|
| Supported | LocalizedRouteAttribute | LocalizedRouteAttribute |
| Supported | LocalizedRouteAttribute | RouteAttribute |
| Supported | LocalizedRouteAttribute | Action |
| Supported | RouteAttribute | LocalizedRouteAttribute |
| Supported | Controller | LocalizedRouteAttribute |

## Setup

> WARNING: We support only the **RouteDataRequestCultureProvider** with the RouteDataStringKey = "culture".

Register the services into the `IServiceCollection`.
```csharp
var supportedCultures = new[]
{
    new CultureInfo("cs-CZ"),
    new CultureInfo("en-US"),
};
services.Configure<RequestLocalizationOptions>(options =>
{
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
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
Register your implementation of the `DynamicRouteValueTransformer` into the endpoints middleware with the following template.
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

options.RequestCultureProviders = new List<IRequestCultureProvider>
{
    new RouteDataRequestCultureProvider() { RouteDataStringKey = "culture" }
};

app.UseRequestLocalization(options);
```

Register the tag helper from the `AspNetCore.Mvc.Routing.Localization` package into the `_ViewImports.cshtml` file, which offers to you localize your routes in a Views.
```csharp
@addTagHelper *, AspNetCore.Mvc.Routing.Localization
```

