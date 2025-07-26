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

In this model, a single database table may contain data of multiple tenants. Each row in these tables have a `TenantId` field which is used to distinguish the tenant data and isolate a tenant's data from other tenant users. To make your entities multi-tenant aware, all you have to do is to implement the `IMultiTenant` interface provided by the ABP Framework:

````csharp
using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace MultiTenancyDemo.Products
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

Follow the *[Get Started tutorial](https://abp.io/docs/latest/get-started/layered-web-application)* to create a new ABP application. Remember to select the "Use separate tenant schema" option since I want to demonstrate it in this article.

## Understanding DbContext Structure

TODO

