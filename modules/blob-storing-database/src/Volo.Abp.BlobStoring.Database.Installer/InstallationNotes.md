# Installation Notes for Blob Storing Database Module

It is typical to store file contents in an application and read these file contents on need. Not only files, but you may also need to save various types of large binary objects, a.k.a. [BLOBs](https://en.wikipedia.org/wiki/Binary_large_object), into a storage. For example, you may want to save user profile pictures.

A BLOB is a typically byte array. There are various places to store a BLOB item; storing in the local file system, in a shared database or on the [Azure BLOB storage](https://azure.microsoft.com/en-us/products/storage/blobs/) can be options.

The ABP provides an abstraction to work with BLOBs and provides some pre-built storage providers that you can easily integrate to. Having such an abstraction has some benefits;

You can easily integrate to your favorite BLOB storage provides with a few lines of configuration.
You can then easily change your BLOB storage without changing your application code.
If you want to create reusable application modules, you don't need to make assumption about how the BLOBs are stored.
ABP BLOB Storage system is also compatible to other ABP features like multi-tenancy.

## Required Dependencies

The Blob Storing Database module depends on the following ABP modules:
- ABP BlobStoring module

## Installation Steps

1. Add the following NuGet packages to your project:
   - `Volo.Abp.BlobStoring.Database.EntityFrameworkCore` (for EF Core)
   - `Volo.Abp.BlobStoring.Database.MongoDB` (for MongoDB)

2. Add the following module dependencies to your module class:

```csharp
[DependsOn(
    typeof(AbpBlobStoringDatabaseEntityFrameworkCoreModule) // Or AbpBlobStoringDatabaseMongoDBModule
)]
public class YourModule : AbpModule
{
}
```

## Database Integration

### EntityFramework Core Configuration

For `EntityFrameworkCore`, add the following configuration to the `OnModelCreating` method of your `DbContext` class:

```csharp
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;

protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    builder.ConfigureBlobStoring();
    
    // ... other configurations
}
```

Then create a new migration and apply it to the database:

```bash
dotnet ef migrations add Added_BlobStoring
dotnet ef database update
```

## 6. **Documentation**

For detailed information and usage instructions, please visit the [BLOB Storing documentation](https://abp.io/docs/latest/framework/infrastructure/blob-storing). 