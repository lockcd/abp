# Installation Notes for Blogging Module

The ABP Blogging module provides a simple blogging system for ABP applications. It allows you to create and manage blogs, posts, tags, and comments. The module includes both a public interface for readers and an admin interface for content management.

Key features of the Blogging module:
- Multiple blog support
- Post management with rich text editing
- Commenting functionality
- Social media sharing
- Admin interface for content management

## Required Dependencies

The Blogging module depends on the following ABP modules:
- ABP Blob Storing module (for media management)

## Installation Steps

1. Add the following NuGet packages to your project:
   - `Volo.Blogging.Application`
   - `Volo.Blogging.HttpApi`
   - `Volo.Blogging.EntityFrameworkCore` (for EF Core)
   - `Volo.Blogging.MongoDB` (for MongoDB)
   - `Volo.Blogging.Web` (for MVC UI)

     // Admin UI
   - `Volo.Blogging.Admin.Application`
   - `Volo.Blogging.Admin.HttpApi`
   - `Volo.Blogging.Admin.Web` (for MVC UI)

2. Add the following module dependencies to your module class:

```csharp
[DependsOn(
    // Other dependencies
    typeof(BloggingApplicationModule),
    typeof(BloggingHttpApiModule),
    typeof(BloggingEntityFrameworkCoreModule), // For EF Core
    typeof(BloggingMongoDbModule) // For MongoDB
    typeof(BloggingWebModule) // For MVC UI
    // Admin UI
    typeof(BloggingAdminApplicationModule),
    typeof(BloggingAdminHttpApiModule),
    typeof(BloggingAdminWebModule) // For MVC UI
)]
public class YourModule : AbpModule
{
}
```

### EntityFramework Core Configuration

For `EntityFrameworkCore`, add the following configuration to the `OnModelCreating` method of your `DbContext` class:

```csharp
using Volo.Blogging.EntityFrameworkCore;

protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    builder.ConfigureBlogging();
    
    // ... other configurations
}
```

Then create a new migration and apply it to the database:

```bash
dotnet ef migrations add Added_Blogging
dotnet ef database update
```

## Documentation

For detailed information and usage instructions, please visit the [Blogging Module documentation](https://abp.io/docs/latest/Modules/Cms-Kit/Blogging). 