# Installation Notes for CMS Kit Module

The ABP CMS Kit module provides a set of reusable Content Management System (CMS) features for your ABP-based applications. It offers ready-to-use UI components and APIs for common content management requirements.

This module is part of the ABP Framework and provides features like comments, ratings, tags, blogs, and more to help you build content-rich applications.

## Required Dependencies

The CMS Kit module depends on the following modules:
- Blob Storing module (for media management)

## Installation Steps

You can manually add the required NuGet packages:

1. Add the following NuGet packages to your project:
   - `Volo.CmsKit.Public.Application` (for public features)
   - `Volo.CmsKit.Public.HttpApi` (for public API)
   - `Volo.CmsKit.Public.Web` (for public UI)
   - `Volo.CmsKit.EntityFrameworkCore` (for EF Core)
   - `Volo.CmsKit.MongoDB` (for MongoDB)
   // admin
   - `Volo.CmsKit.Admin.Application` (for admin features)
   - `Volo.CmsKit.Admin.HttpApi` (for admin API)
   - `Volo.CmsKit.Admin.Web` (for admin UI)



2. Add the following module dependencies to your module class:

```csharp
[DependsOn(
    typeof(CmsKitAdminApplicationModule), // For admin features
    typeof(CmsKitPublicApplicationModule), // For public features
    typeof(CmsKitAdminHttpApiModule), // For admin API
    typeof(CmsKitPublicHttpApiModule), // For public API
    typeof(CmsKitAdminWebModule), // For admin UI
    typeof(CmsKitPublicWebModule), // For public UI
    typeof(CmsKitEntityFrameworkCoreModule) // For EF Core
    // OR typeof(CmsKitMongoDbModule) // For MongoDB
)]
public class YourModule : AbpModule
{
}
```

### EntityFramework Core Configuration

For `EntityFrameworkCore`, further configuration is needed in the `OnModelCreating` method of your `DbContext` class:

```csharp
using Volo.CmsKit.EntityFrameworkCore;

protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    builder.ConfigureCmsKit();
    
    //...
}
```

Also, you will need to create a new migration and apply it to the database:

```bash
dotnet ef migrations add Added_CmsKit
dotnet ef database update
```

## Feature Management

By default, all CMS Kit features are disabled. You need to enable the features you want to use in your application. Open the `GlobalFeatureConfigurator` class in the `Domain.Shared` project and add the following code to the `Configure` method:

```csharp
GlobalFeatureManager.Instance.Modules.CmsKit(cmsKit =>
{
    // Enable all features
    cmsKit.EnableAll();
    
    // Or enable specific features
    // cmsKit.Comments.Enable();
    // cmsKit.Tags.Enable();
    // cmsKit.Ratings.Enable();
    // cmsKit.Reactions.Enable();
    // cmsKit.Blogs.Enable();
    // cmsKit.Pages.Enable();
    // cmsKit.MediaDescriptors.Enable();
});
```

## Documentation

For detailed information and usage instructions, please visit the [CMS Kit documentation](https://abp.io/docs/latest/modules/cms-kit). 