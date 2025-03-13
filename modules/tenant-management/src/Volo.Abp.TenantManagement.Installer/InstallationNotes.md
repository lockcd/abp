# Installation Notes for Tenant Management Module

The ABP Tenant Management module provides multi-tenancy features for your ABP-based applications. It implements the `ITenantStore` interface and provides UI to manage tenants and their features.

This module is part of the ABP Framework and provides the core functionality needed to build multi-tenant (SaaS) applications.

## Installation Steps

The Tenant Management module is pre-installed in the ABP startup templates. If you need to manually install it, follow these steps:

1. Add the following NuGet packages to your project:
   - `Volo.Abp.TenantManagement.Application`
   - `Volo.Abp.TenantManagement.HttpApi`
   - `Volo.Abp.TenantManagement.EntityFrameworkCore` (for EF Core)
   - `Volo.Abp.TenantManagement.MongoDB` (for MongoDB)
   - `Volo.Abp.TenantManagement.Web` (for MVC UI)
   - `Volo.Abp.TenantManagement.Blazor.Server` (for Blazor Server UI)
   - `Volo.Abp.TenantManagement.Blazor.WebAssembly` (for Blazor Web Assembly UI)

2. Add the following module dependencies to your module class:

```csharp
[DependsOn(
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpTenantManagementHttpApiModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule) // For EF Core
    typeof(AbpTenantManagementMongoDbModule) // For MongoDB
    typeof(AbpTenantManagementWebModule), // For MVC UI
    typeof(AbpTenantManagementBlazorServerModule), // For Blazor Server UI
    typeof(AbpTenantManagementBlazorWebAssemblyModule) // For Blazor Web Assembly UI
)]
public class YourModule : AbpModule
{
}
```

### EntityFramework Core Configuration

For `EntityFrameworkCore`, further configuration is needed in the `OnModelCreating` method of your `DbContext` class:

```csharp
using Volo.Abp.TenantManagement.EntityFrameworkCore;

//...

protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    builder.ConfigureTenantManagement();
    
    //...
}
```

Also, you will need to create a new migration and apply it to the database:

```bash
dotnet ef migrations add Added_TenantManagement
dotnet ef database update
```

## Documentation

For detailed information and usage instructions, please visit the [Tenant Management module documentation](https://abp.io/docs/latest/modules/tenant-management). 