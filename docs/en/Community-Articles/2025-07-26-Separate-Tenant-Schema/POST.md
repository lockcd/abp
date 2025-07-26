# Multi-Tenancy with Separate Databases in .NET and ABP Framework

[Multi-tenancy](https://abp.io/architecture/multi-tenancy) is a common architectural concept for modern SaaS applications, enabling a single application to serve multiple customers (each known as a tenant) while maintaining data isolation, scalability, and operational efficiency. The "Separate database per tenant" approach offers the highest level of data isolation, making it ideal for scenarios with strict data privacy, security, and performance requirements.

In this article, we’ll explore how to use this advanced multi-tenancy model using the powerful capabilities of the ABP Framework and the .NET platform.

> In this article, I will use ABP Studio for creating the application. ABP Studio allows to select "separate database per tenant" option only for [commercial licenses](https://abp.io/pricing).

## Understanding Database Models for a Multi-Tenant Application

In the next sections, I will explain various models for database models of a multi-tenant solution:

* Single (shared) Database Model
* Separate Tenant Databases Model
* Hybrid Multi-Tenant Database Model

Let's start with the first one...

### Single (shared) Database Model

In the shared database model, all the application data stored in a single physical database. In the following diagram, you see different kind of users use the application, and the application stored their data in a main database:

![single-shared-database](single-shared-database.png)

This is the default behavior when you [create a new ABP application](https://abp.io/docs/latest/get-started), because it is simple to begin with and proper for must applications.

In this model, a single database table may contain data of multiple tenants. Each row in these tables have a `TenantId` field which is used to distinguish the tenant data and isolate a tenant's data from other tenant users. To make your entities multi-tenant aware, all you have to do is to implement the `IMultiTenant` interface provided by the ABP Framework.

Here, is an example `Product` entity that should support multi-tenancy:

````csharp
using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace MtDemoApp
{
    public class Product : AggregateRoot<Guid>, IMultiTenant //Implementing the interface
    {
        public Guid? TenantId { get; set; } //Defined by the IMultiTenant interface
        public string Name { get; set; }
        public float Price { get; set; }
    }
}
````

In this way, ABP Framework automatically isolates data using the `TenantId` property. You don't need to care about how to set `TenantId` or filter data when you need to query from database - all automated.

### Separate Tenant Databases Model

In the separate tenant database model, each tenant has a dedicated physical database (with a separate connection string), as shown below:

![separate-tenant-database-multi-tenancy](separate-tenant-database-multi-tenancy.png)

ABP Framework can automatically select the right database from the current user's tenant context. Again, it is completely automated. You just need to set a connection string for a tenant, as we will do later in this article.

Even each tenant has a separate database, we still need to a main database to store host-side data, like a table of tenants, their connection strings and some other management data for tenants. Also, tenant-independent (or tenant-shared) application data is stored in the main database.

### Hybrid Multi-Tenant Database Model

Lastly, you may want to have a hybrid model, where some tenants shares a single database (they don't have separate databases) but some of them have dedicated databases. In the following figure, Tenant C has its own physical database, but all other tenants data stored in the main database of the application.

![hybrid-database-multi-tenancy](hybrid-database-multi-tenancy.png)

ABP Framework handles the complexity: If a tenant has a separate database it uses that tenant's database, otherwise it filters the tenant data by the `TenantId` field in shared tables.

## Understanding the Separate Tenant Schema Approach

When you create a new ABP solution, it has a single `DbContext` class (for Entity Framework Core) by default. It also includes the necessary EF Core code-first database migrations to create and update the database. As a result of this approach, the main database schema (tables and their fields) will be identical with a tenant database schema. As a drawback of that, tenant databases have some tables that are not meaningful and not used. For example, Tenants table (a list of tenants) will be created in the tenant database, but will never be used (because tenant list is stored in the main database).

As a solution to that problem, ABP Studio provides a "Use separate tenant schema" option on the Multi-Tenancy step of the solution creation wizard:

![separate-tenant-schema-option](separate-tenant-schema-option.png)

This option is only available for the [Layered Monolith (optionally Modular) Solution Template](https://abp.io/docs/latest/get-started/layered-web-application). We don't provide that option in other templates, because:

* [Single-Layer](https://abp.io/docs/latest/get-started/single-layer-web-application) template is recommended for more simpler applications with an easy-to-understand architecture. We don't want to add these kind of complications in that template.
* [Microservice](https://abp.io/docs/latest/get-started/microservice) template already has a separate database for each service. Having multiple database schema (and multiple `DbContext` classes) for each service makes it over complicated without bringing much value.

While you can manually convert your applications so they support separate database schema approach (ABP is flexible), it is not recommended to do it for these solution types.

> Note that "Separate database per tenant" approach is already supported by default for the Single-Layer template too. "Separate tenant schema" is something different as I explained in this section.

## Creating a new Application

Follow the *[Get Started tutorial](https://abp.io/docs/latest/get-started/layered-web-application)* to create a new ABP application. Remember to select the "*Use separate tenant schema*" option since I want to demonstrate it in this article.

## Understanding the DbContext Structure

When you open the solution in your IDE, you will see the following structure under the `.EntityFrameworkCore` project:

![multi-tenancy-dbcontext-structure](multi-tenancy-dbcontext-structure.png)

There are 3 DbContext-related classes here (MtDemoApp is your application name):

* `MtDemoAppDbContext` class is used to map entities for the main (host + shared) database.
* `MtDemoAppTenantDbContext` class is used to map entities for tenant that have separate physical databases.
* `MtDemoAppDbContextBase` is an abstract base class for the classes explained above. In this way, you can configure common mapping logic here.

Let's see these classes a bit closer...

### The Main `DbContext` Class

Here the main `DbContext` class:

````csharp
public class MtDemoAppDbContext : MtDemoAppDbContextBase<MtDemoAppDbContext>
{
    public MtDemoAppDbContext(DbContextOptions<MtDemoAppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.SetMultiTenancySide(MultiTenancySides.Both);

        base.OnModelCreating(builder);
    }
}
````

* It inherits from the `MtDemoAppDbContextBase` as I mentioned before. So, any configuration made in the base class is also valid here.
* `OnModelCreating` overrides the base method and sets the multi-tenancy side as `MultiTenancySides.Both`. `Both` means this database can store host data as well as tenant data. This is needed because we store data in this database for the tenants who don't have a separate database.

### The Tenant `DbContext` class

Here is the tenant-specific `DbContext` class:

````csharp
public class MtDemoAppTenantDbContext : MtDemoAppDbContextBase<MtDemoAppTenantDbContext>
{
    public MtDemoAppTenantDbContext(DbContextOptions<MtDemoAppTenantDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.SetMultiTenancySide(MultiTenancySides.Tenant);

        base.OnModelCreating(builder);
    }
}
````

The only difference is that we used `MultiTenancySides.Tenant` as the multi-tenancy side here, since this `DbContext` will only have entities/tables for tenants that have separate databases.

### The Base `DbContext` Class

Here is the base `DbContext` class:

````csharp
public abstract class MtDemoAppDbContextBase<TDbContext> : AbpDbContext<TDbContext>
    where TDbContext : DbContext
{
    
    public MtDemoAppDbContextBase(DbContextOptions<TDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentityPro();
        builder.ConfigureOpenIddictPro();
        builder.ConfigureFeatureManagement();
        builder.ConfigureLanguageManagement();
        builder.ConfigureSaas();
        builder.ConfigureTextTemplateManagement();
        builder.ConfigureBlobStoring();
        builder.ConfigureGdpr();

        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(MtDemoAppConsts.DbTablePrefix + "YourEntities", MtDemoAppConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});

        //if (builder.IsHostDatabase())
        //{
        //    /* Tip: Configure mappings like that for the entities only
               * available in the host side,
        //     * but should not be in the tenant databases. */
        //}
    }
}
````

This `DbContext` class configures database mappings for all the [application modules](https://abp.io/docs/latest/modules) used by this application by calling their extension methods, like `builder.ConfigureBackgroundJobs()`. Each of these extension methods are defined as multi-tenancy aware and care about what you've set for the multi-tenancy side.

### Where to Configure Your Entities?

You can configure your entity mappings in the `OnModelCreating` method in any of the `DbContext` classes that was explained:

* If you configure in the main `DbContext` class, these configuration will be valid only for the main database. So, don't configure tenant-related configuration here, otherwise, it won't be applied for the tenants who have separate databases.
* If you configure in the tenant `DbContext` class, it will be valid only for the tenants with separate databases. You rarely need to do that. You typically want to make same configuration in the base `DbContext` to support hybrid scenarios (some tenants use the main (shared) database and some tenants have separate databases).
* If you configure in the base `DbContext` class, it will be valid for the main database and tenant databases. You typically define tenant-related configuration here. That means, if you have a multi-tenant `Product` entity, then you should define its EF Core database mapping configuration here, so the Products table is created in the main database as well as in the tenant databases.

The recommended approach is to configure all the mapping in the base class, but add controls like `builder.IsHostDatabase()` and `builder.IsTenantDatabase()` to conditionally configure the mappings:

![builder-check-tenant-side](builder-check-tenant-side.png)

## Adding Database Migrations

In this section, I will show how to configure your entity mappings, generate database migrations and apply to the database.

### Defining an Entity

Let's define a `Product` entity in the `.Domain` layer of your application:

````csharp
using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace MtDemoApp
{
    public class Product : AggregateRoot<Guid>, IMultiTenant
    {
        public Guid? TenantId { get; set; }
        public string Name { get; set; }
        public float Price { get; set; }
    }
}
````

### Add a New Database Migration

TODO