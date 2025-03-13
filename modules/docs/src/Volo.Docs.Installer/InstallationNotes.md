# Installation Notes for Docs Module

The ABP Docs module provides a complete documentation system for ABP applications. It allows you to create, manage, and publish documentation from various sources like GitHub, GitLab, or local file system. The module includes both a public interface for readers and an admin interface for documentation management.

Key features of the Docs module:
- Multiple documentation projects support
- Version control integration
- Markdown support
- Navigation generation
- Full-text search
- Multi-language support
- Admin interface for documentation management

## Installation Steps

1. Add the following NuGet packages to your project:
   - `Volo.Docs.Application`
   - `Volo.Docs.HttpApi`
   - `Volo.Docs.Web` (for MVC UI)
   - `Volo.Docs.EntityFrameworkCore` (for EF Core)
   - `Volo.Docs.MongoDB` (for MongoDB)

   For the admin UI:
   - `Volo.Docs.Admin.Application`
   - `Volo.Docs.Admin.HttpApi`
   - `Volo.Docs.Admin.Web`

2. Add the following module dependencies to your module class:

```csharp
[DependsOn(
    typeof(DocsApplicationModule),
    typeof(DocsHttpApiModule),
    typeof(DocsWebModule), // For MVC UI
    typeof(DocsEntityFrameworkCoreModule), // Or DocsMongoDbModule
    typeof(DocsAdminApplicationModule),
    typeof(DocsAdminHttpApiModule),
    typeof(DocsAdminWebModule)
)]
public class YourModule : AbpModule
{
}
```

## EntityFramework Core Configuration

For `EntityFrameworkCore`, add the following configuration to the `OnModelCreating` method of your `DbContext` class:

```csharp
using Volo.Docs.EntityFrameworkCore;

protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    builder.ConfigureDocs();
    
    // ... other configurations
}
```

Then create a new migration and apply it to the database:

```bash
dotnet ef migrations add Added_Docs
dotnet ef database update
```

## Documentation

For detailed information and usage instructions, please visit the [Docs Module documentation](https://abp.io/docs/latest/Modules/Docs). 