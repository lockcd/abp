# Installation Notes for Setting Management Module

The Setting Management module provides a way to store and manage settings in an ABP application. Settings are used to store application, tenant, or user-specific configuration values that can be changed at runtime. The module includes both a UI for setting management and an API for programmatic setting management.

Key features of the Setting Management module:
- Store and retrieve settings
- Multi-level setting management (global, tenant, user)
- Setting management UI
- Extensible setting provider system
- Multi-tenancy support
- Integration with other ABP modules

## NuGet Packages

The following NuGet packages are required for the Setting Management module:
- `Volo.Abp.SettingManagement.Application`
- `Volo.Abp.SettingManagement.HttpApi`
- `Volo.Abp.SettingManagement.EntityFrameworkCore` (for EF Core)
- `Volo.Abp.SettingManagement.MongoDB` (for MongoDB)
- `Volo.Abp.SettingManagement.Web` (for MVC UI)
- `Volo.Abp.SettingManagement.Blazor.Server` (for Blazor Server UI)
- `Volo.Abp.SettingManagement.Blazor.WebAssembly` (for Blazor WebAssembly UI)

## Module Dependencies

```csharp
[DependsOn(
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpSettingManagementHttpApiModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementMongoDbModule), // If using MongoDB
    typeof(AbpSettingManagementWebModule), // For MVC UI
    typeof(AbpSettingManagementBlazorServerModule), // For Blazor Server UI
    typeof(AbpSettingManagementBlazorWebAssemblyModule) // For Blazor WebAssembly UI
)]
public class YourModule : AbpModule
{
}
```

## EntityFramework Core Configuration

For `EntityFrameworkCore`, add the following configuration to the `OnModelCreating` method of your `DbContext` class:

```csharp
using Volo.Abp.SettingManagement.EntityFrameworkCore;

protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    builder.ConfigureSettingManagement();
    
    // ... other configurations
}
```

Then create a new migration and apply it to the database:

```bash
dotnet ef migrations add Added_SettingManagement
dotnet ef database update
```

## 6. **Documentation**

For detailed information and usage instructions, please visit the [Setting Management Module documentation](https://abp.io/docs/latest/Modules/Setting-Management). 