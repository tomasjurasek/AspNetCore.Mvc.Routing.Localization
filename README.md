# AspNetCore.Mvc.Routing.Localization
## Requirements
#### RequestCultureProviders
You need to setup the RouteDataRequestCultureProvider with RouteDataStringKey = "culture".
#### DynamicRouteValueTransformer
Implement and register the DynamicRouteValueTransformer.
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

## Setup
Register the services into the `IServiceCollection`.
```csharp
 services.AddLocalizedRouting();
```

Remove the default AnchorTagHelper and register from the library in the `_ViewImports.cshtml` file.
```csharp
@addTagHelper *, AspNetCore.Mvc.Routing.Localization
@removeTagHelper Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper, Microsoft.AspNetCore.Mvc.TagHelpers
```

## Using

Use the `LocalizedRoute` attribute where the parameters are culture, which is registered supported culture and the template. 
You can also combine with the `Route` attribute.
