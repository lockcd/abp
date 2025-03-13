# Installation Notes for OpenIddict Module

The OpenIddict module is an authentication module for the ABP Framework that provides OAuth 2.0 and OpenID Connect server capabilities. It is built on the OpenIddict library and provides a complete solution for implementing authentication and authorization in your ABP applications.

Key features of the OpenIddict module:
- OAuth 2.0 and OpenID Connect server implementation
- Token generation and validation
- Authorization code, implicit, client credentials, and resource owner password flows
- JWT and reference token support
- Client application management
- Scope management
- Integration with ABP's permission system

## Required Dependencies

The OpenIddict module depends on the following ABP modules:
- ABP Identity module (for user management)

## Installation Steps

1. Add the following NuGet packages to your project:
   - `Volo.Abp.OpenIddict.Domain`
   - `Volo.Abp.OpenIddict.Domain.Shared`
   - `Volo.Abp.OpenIddict.AspNetCore`
   - `Volo.Abp.OpenIddict.EntityFrameworkCore` (for EF Core)
   - `Volo.Abp.OpenIddict.MongoDB` (for MongoDB)
   - `Volo.Abp.PermissionManagement.Domain.Identity` (for permission management)
   - `Volo.Abp.PermissionManagement.Domain.OpenIddict` (for permission management)

2. Add the following module dependencies to your module class:

```csharp
[DependsOn(
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule), // Or AbpOpenIddictMongoDbModule
    typeof(AbpPermissionManagementDomainIdentityModule), // For permission management
    typeof(AbpPermissionManagementDomainOpenIddictModule) // For permission management
)]
public class YourModule : AbpModule
{
}
```

### Database Integration

#### EntityFramework Core Configuration

For `EntityFrameworkCore`, add the following configuration to the `OnModelCreating` method of your `DbContext` class:

```csharp
using Volo.Abp.OpenIddict.EntityFrameworkCore;

protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    builder.ConfigureIdentity();
    builder.ConfigureOpenIddict();
    
    // ... other configurations
}
```

Then create a new migration and apply it to the database:

```bash
dotnet ef migrations add Added_OpenIddict
dotnet ef database update
```

## Configuration

Configure the OpenIddict module in your module's `ConfigureServices` method:

```csharp
public override void PreConfigureServices(ServiceConfigurationContext context)
{
        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("AbpSolution134");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

            PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
            {
                serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx", configuration["AuthServer:CertificatePassPhrase"]!);
            });
        }

        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
}
```

## Documentation

For detailed information and usage instructions, please visit the [OpenIddict Module documentation](https://abp.io/docs/latest/Modules/OpenIddict). 