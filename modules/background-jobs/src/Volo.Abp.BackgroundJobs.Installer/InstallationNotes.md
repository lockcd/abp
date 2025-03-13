# Installation Notes for Background Jobs Module

Background jobs are used to queue some tasks to be executed in the background. You may need background jobs for several reasons. Here are some examples:

- To perform **long-running tasks** without having the users wait. For example, a user presses a 'report' button to start a long-running reporting job. You add this job to the **queue** and send the report's result to your user via email when it's completed.
- To create **re-trying** and **persistent tasks** to **guarantee** that a code will be **successfully executed**. For example, you can send emails in a background job to overcome **temporary failures** and **guarantee** that it eventually will be sent. That way users do not wait while sending emails.

Background jobs are **persistent** that means they will be **re-tried** and **executed** later even if your application crashes.

## Installation Steps

1. Add the following NuGet packages to your project:
   - `Volo.Abp.BackgroundJobs.EntityFrameworkCore` (for EF Core)
   - `Volo.Abp.BackgroundJobs.MongoDB` (for MongoDB)

2. Add the following module dependencies to your module class:

```csharp
[DependsOn(
    typeof(AbpBackgroundJobsDomainModule),
    typeof(AbpBackgroundJobsDomainSharedModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule) // For EF Core
    // OR typeof(AbpBackgroundJobsMongoDbModule) // For MongoDB
)]
public class YourModule : AbpModule
{
}
```

## EntityFramework Core Configuration

For `EntityFrameworkCore`, further configuration is needed in the `OnModelCreating` method of your `DbContext` class:

```csharp
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;

protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    builder.ConfigureBackgroundJobs();
    
    //...
}
```

Also, you will need to create a new migration and apply it to the database:

```bash
dotnet ef migrations add Added_BackgroundJobs
dotnet ef database update
```

## Documentation

For detailed information and usage instructions, please visit the [Background Jobs documentation](https://abp.io/docs/latest/framework/infrastructure/background-jobs). 