# Installation Notes for Identity Module

The ABP Identity module provides user and role management functionality for your ABP-based applications. It is built on Microsoft's Identity library and extends it with additional features like organization units and claims management.

This module is part of the ABP Framework and provides the core identity management capabilities needed by most business applications.

## Installation Steps

The Identity module is pre-installed in the ABP startup templates. If you need to manually install it, follow these steps:

1. Add the following NuGet packages to your project:
   - `Volo.Abp.Identity.Application`
   - `Volo.Abp.Identity.HttpApi`
   - `Volo.Abp.Identity.Web` (for MVC UI)
   - `Volo.Abp.Identity.Blazor.Server` (for Blazor Server UI)
   - `Volo.Abp.Identity.Blazor.WebAssembly` (for Blazor Web Assembly UI)
   - `Volo.Abp.Identity.EntityFrameworkCore` (for EF Core)
   - `Volo.Abp.Identity.MongoDB` (for MongoDB)
   - `Volo.Abp.PermissionManagement.Domain.Identity` (for permission management)

2. Add the following module dependencies to your module class:

```csharp
[DependsOn(
    typeof(AbpIdentityApplicationModule),
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpIdentityWebModule), // For MVC UI
    typeof(AbpIdentityBlazorServerModule), // For Blazor Server UI
    typeof(AbpIdentityBlazorWebAssemblyModule), // For Blazor Web Assembly UI
    typeof(AbpIdentityEntityFrameworkCoreModule) // For EF Core
    typeof(AbpPermissionManagementDomainIdentityModule) // For permission management
    // OR typeof(AbpIdentityMongoDbModule) // For MongoDB
)]
public class YourModule : AbpModule
{
}
```

## EntityFramework Core Configuration

For `EntityFrameworkCore`, further configuration is needed in the `OnModelCreating` method of your `DbContext` class:

```csharp
using Volo.Abp.Identity.EntityFrameworkCore;

//...

protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    builder.ConfigureIdentity();
    
    //...
}
```

Also, you will need to create a new migration and apply it to the database:

```bash
dotnet ef migrations add Added_Identity
dotnet ef database update
```

## Documentation

For detailed information and usage instructions, please visit the [Identity module documentation](https://abp.io/docs/latest/modules/identity). 
