# Installation Notes for Audit Logging Module

The ABP Audit Logging module provides automatic audit logging for web requests, service methods, and entity changes. It helps you track user activities and changes in your application.

This module is part of the ABP Framework and provides comprehensive audit logging capabilities including entity history tracking, exception logging, and user activity monitoring.

## Installation Steps

1. Add the following NuGet packages to your project:
   - `Volo.Abp.AuditLogging.EntityFrameworkCore` (for EF Core)
   - `Volo.Abp.AuditLogging.MongoDB` (for MongoDB)

2. Add the following module dependencies to your module class:

```csharp
[DependsOn(
    typeof(AbpAuditLoggingEntityFrameworkCoreModule) // For EF Core
    typeof(AbpAuditLoggingMongoDbModule) // For MongoDB
)]
public class YourModule : AbpModule
{
}
```

## EntityFramework Core Configuration

For `EntityFrameworkCore`, further configuration is needed in the `OnModelCreating` method of your `DbContext` class:

```csharp
using Volo.Abp.AuditLogging.EntityFrameworkCore;

//...

protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    builder.ConfigureAuditLogging();
    
    //...
}
```

Also, you will need to create a new migration and apply it to the database:

```bash
dotnet ef migrations add Added_AuditLogging
dotnet ef database update
```

Configure the audit logging middleware in your `OnApplicationInitialization` method:

```csharp
public override void OnApplicationInitialization(ApplicationInitializationContext context)
{
    var app = context.GetApplicationBuilder();
    
    app.UseAuditing(); // Enable audit logging middleware
    
    //...
}
```

## Documentation

For detailed information and usage instructions, please visit the [Audit Logging documentation](https://abp.io/docs/latest/framework/infrastructure/audit-logging).