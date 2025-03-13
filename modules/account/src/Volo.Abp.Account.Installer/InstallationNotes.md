# Installation Notes for Account Module

Account module implements the basic authentication features like login, register, forgot password and account management.

This module is based on [Microsoft's Identity library](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-6.0&tabs=visual-studio) and the [Identity Module](https://docs.abp.io/en/abp/latest/modules/identity). It has [IdentityServer](https://docs.abp.io/en/abp/latest/modules/identity-server) integration (based on the [IdentityServer Module](https://docs.abp.io/en/abp/latest/modules/identity-server)) and [OpenIddict](https://github.com/openiddict) integration (based on the [Openiddict Module](https://docs.abp.io/en/abp/latest/modules/openiddict)) to provide single sign-on, access control and other advanced authentication features.

## Required Dependencies

The Account module depends on the following modules:
- Identity Module
- Either OpenIddict Module or IdentityServer Module (for single sign-on capabilities)

## Installation Steps

The Account module is pre-installed in the ABP startup templates. If you need to manually install it, follow these steps:

1. Add the following NuGet packages to your project:
   - `Volo.Abp.Account.Application`
   - `Volo.Abp.Account.HttpApi`
   - `Volo.Abp.Account.Web` (for MVC UI)
   - `Volo.Abp.Account.Blazor` (for MVC UI)

2. Add the following module dependencies to your module class:

```csharp
[DependsOn(
    typeof(AbpAccountApplicationModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpAccountEntityFrameworkCoreModule),
    typeof(AbpAccountWebModule) // For MVC UI
    typeof(AbpAccountBlazorModule ) // For BLAZOR UI
)]
public class YourModule : AbpModule
{
}
```

3. For OpenIddict integration, add the `Volo.Abp.Account.Web.OpenIddict` package and its module dependency:

```csharp
[DependsOn(
    // Other dependencies
    typeof(AbpAccountWebOpenIddictModule)
)]
public class YourModule : AbpModule
{
}
```

4. For IdentityServer integration, add the `Volo.Abp.Account.Web.IdentityServer` package and its module dependency:

```csharp
[DependsOn(
    // Other dependencies
    typeof(AbpAccountWebIdentityServerModule)
)]
public class YourModule : AbpModule
{
}
```

## Documentation

For detailed information and usage instructions, please visit the [Account Module documentation](https://abp.io/docs/latest/Modules/Account). 