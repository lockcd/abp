# Installation Notes for Permission Management Module

The ABP Permission Management module provides infrastructure to persist and manage permissions in your ABP-based applications. It allows you to grant permissions to users, roles, or other entities.

This module is part of the ABP Framework and provides the core permission management functionality needed by most business applications.

## Installation Steps

The Permission Management module is pre-installed in the ABP startup templates. If you need to manually install it, follow these steps:

1. Add the following NuGet packages to your project:
   - `Volo.Abp.PermissionManagement.Application`
   - `Volo.Abp.PermissionManagement.HttpApi`
   - `Volo.Abp.PermissionManagement.Web` (for MVC UI)
   - `Volo.Abp.PermissionManagement.Blazor.Server` (for Blazor Server UI)
   - `Volo.Abp.PermissionManagement.Blazor.WebAssembly` (for Blazor Web Assembly UI)
   - `Volo.Abp.PermissionManagement.EntityFrameworkCore` (for EF Core)
   - `Volo.Abp.PermissionManagement.MongoDB` (for MongoDB)

2. Add the following module dependencies to your module class:

```csharp
[DependsOn(
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpPermissionManagementHttpApiModule),
    typeof(AbpPermissionManagementWebModule), // For MVC UI
    typeof(AbpPermissionManagementBlazorServerModule), // For Blazor Server UI
    typeof(AbpPermissionManagementBlazorWebAssemblyModule), // For Blazor Web Assembly UI
    typeof(AbpPermissionManagementEntityFrameworkCoreModule) // For EF Core
    typeof(AbpPermissionManagementMongoDbModule) // For MongoDB
)]
public class YourModule : AbpModule
{
}
```

### EntityFramework Core Configuration

For `EntityFrameworkCore`, further configuration is needed in the `OnModelCreating` method of your `DbContext` class:

```csharp
using Volo.Abp.PermissionManagement.EntityFrameworkCore;

protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    builder.ConfigurePermissionManagement();
    
    //...
}
```

Also, you will need to create a new migration and apply it to the database:

```bash
dotnet ef migrations add Added_PermissionManagement
dotnet ef database update
```

## Documentation

For detailed information and usage instructions, please visit the [Permission Management module documentation](https://abp.io/docs/latest/modules/permission-management). 