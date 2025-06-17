# ABP Version 9.3 Migration Guide

This document is a guide for upgrading ABP v9.2 solutions to ABP v9.3. There are some changes in this version that may affect your applications, please read it carefully and apply the necessary changes to your application.

## Updated `RabbitMQ.Client` to `7.x`

In this version, we updated `RabbitMQ.Client` to `7.1.2`. [This is a major version update](https://github.com/rabbitmq/rabbitmq-dotnet-client/blob/main/v7-MIGRATION.md) that brings significant improvements to the library:

1. Full async/await support throughout the entire public API and internals
2. Improved performance and resource utilization
3. Better error handling and connection management

With this update, you should update your method calls to use the new async/await support (in the RabbitMQ related provider packages). There are some method signature changes and new API calls, aligned with the new API. You can see the internal changes we made in [#22510](https://github.com/abpframework/abp/pull/22510) and make the relevant changes in your code.

## Docs Module: Export as PDF

In this version, we have introduced a new feature to the [Docs Module](../../modules/docs.md) that allows you to export the documentation as a PDF file. (Administrators generate PDF files from the back-office side, and then "Download PDF" button appears on the document system, allowing users to download the compiled documentation as a PDF file.)

While implementing this feature, we have made changes in some services of the Docs Module. Typically, you don't need to make any changes in your code unless you have overridden or used internal services of the Docs Module. 

For example, the `ProjectAdminAppService` constructor has been changed to accept a new parameter:

```diff
public class ProjectAdminAppService : ApplicationService, IProjectAdminAppService
{
    public ProjectAdminAppService(
        IProjectRepository projectRepository,
        IDocumentRepository documentRepository,
        IDocumentFullSearch elasticSearchService,
        IGuidGenerator guidGenerator,
+       IProjectPdfFileStore projectPdfFileStore)
}
```

You can see the all internal changes we made in [#22430](https://github.com/abpframework/abp/pull/22430) and [#22922](https://github.com/abpframework/abp/pull/22922) and make the relevant changes in your code if needed. 

## Angular UI: Migrating NPM Packages to Standalone Structure

In this version, we've updated our Angular packages to support the new standalone components architecture. This is a non-breaking change - your existing module-based applications will continue to work without any modifications. However, if you wish to migrate to the standalone approach, [we've provided the necessary updates in our packages](https://github.com/abpframework/abp/pull/22829).

The main changes include:
- Updated routing configurations to support both module-based and standalone approaches
- Added support for standalone components in ABP Suite code generation
- Updated schematics to support both module-based and standalone templates

For detailed migration steps and best practices, please refer to our upcoming documentation and/or blog post. The migration is optional, and you can continue using the module-based approach if you prefer.