# ABP Version 9.3 Migration Guide

This document is a guide for upgrading ABP v9.2 solutions to ABP v9.3. There are some changes in this version that may affect your applications, please read it carefully and apply the necessary changes to your application.

## Open-Source (Framework)

### Updated `RabbitMQ.Client` to `7.x`

In this version, we updated `RabbitMQ.Client` to `7.1.2`. [This is a major version update](https://github.com/rabbitmq/rabbitmq-dotnet-client/blob/main/v7-MIGRATION.md) that brings significant improvements to the library:

1. Full async/await support throughout the entire public API and internals
2. Improved performance and resource utilization
3. Better error handling and connection management

With this update, you should update your method calls to use the new async/await support (in the RabbitMQ related provider packages). There are some method signature changes and new API calls, aligned with the new API. You can see the internal changes we made in [#22510](https://github.com/abpframework/abp/pull/22510) and make the relevant changes in your code.

### Docs Module: Export as PDF

In this version, we have introduced a new feature to the [Docs Module](../../modules/docs.md) that allows you to export the documentation as a PDF file. (Adminstrators generate PDF files from the back-office side, and then "Download PDF" button appears on the document system, allowing users to download the compiled documentation as a PDF file.)

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

## PRO

> Please check the **Open-Source (Framework)** section before reading this section. The listed topics might affect your application and you might need to take care of them.

### Angular UI: Migrating NPM Packages to Standalone Structure

//TODO: ...