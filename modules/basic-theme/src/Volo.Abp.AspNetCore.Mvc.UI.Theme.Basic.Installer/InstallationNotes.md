# Installation Notes for Basic Theme Module (MVC)

The Basic Theme is a theme implementation for the ASP.NET Core MVC / Razor Pages UI. It is a minimalist theme that doesn't add any styling on top of the plain [Bootstrap](https://getbootstrap.com/). You can take the Basic Theme as the base theme and build your own theme or styling on top of it. See the Customization section.

The Basic Theme has RTL (Right-to-Left language) support.

If you are looking for a professional, enterprise ready theme, you can check the [Lepton Theme](https://abp.io/themes), which is a part of the ABP.

See the [Theming document](https://github.com/abpframework/abp/blob/rel-9.1/docs/en/framework/ui/mvc-razor-pages/theming.md) to learn about themes.

## Installation Steps

The Basic Theme module is pre-installed in the ABP MVC startup templates. If you need to manually install it, follow these steps:

1. Add the following NuGet package to your project:
   - `Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic`

2. Add the following module dependency to your module class:

```csharp
[DependsOn(
    typeof(AbpAspNetCoreMvcUiThemeBasicModule)
)]
public class YourModule : AbpModule
{
}
```

## Documentation

For detailed information and usage instructions, please visit the [Basic Theme documentation](https://abp.io/docs/latest/framework/ui/mvc-razor-pages/basic-theme).