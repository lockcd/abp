# Installation Notes for Basic Theme Module (Blazor)

The Basic Theme is a theme implementation for the Blazor UI. It is a minimalist theme that doesn't add any styling on top of the plain [Bootstrap](https://getbootstrap.com/). You can take the Basic Theme as the base theme and build your own theme or styling on top of it. See the Customization section.

## Installation Steps

The Basic Theme module is pre-installed in the ABP Blazor startup templates. If you need to manually install it, follow these steps:

1. Add the following NuGet packages to your project based on your Blazor hosting model:

   **For Blazor Server:**
   - `Volo.Abp.AspNetCore.Components.Server.BasicTheme`

   **For Blazor WebAssembly:**
   - `Volo.Abp.AspNetCore.Components.WebAssembly.BasicTheme`

2. Add the following module dependencies to your module class:

   **For Blazor Server:**
   ```csharp
   [DependsOn(
       typeof(AbpAspNetCoreComponentsServerBasicThemeModule),
   )]
   public class YourModule : AbpModule
   {
   }
   ```

   **For Blazor WebAssembly:**
   ```csharp
   [DependsOn(
       typeof(AbpAspNetCoreComponentsWebAssemblyBasicThemeModule),
   )]
   public class YourModule : AbpModule
   {
   }
   ```

## Documentation

For detailed information and usage instructions, please visit the [Blazor UI Basic Theme documentation](https://abp.io/docs/latest/framework/ui/blazor/basic-theme?UI=BlazorServer). 