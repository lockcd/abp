using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.MultiTenancy;
using Xunit;
using Shouldly;

public class ConfigurationTenantResolveContributor_Tests : MultiTenancyTestBase
{
    [Fact]
    public async Task Should_Resolve_Tenant_From_Configuration()
    {
        var context = CreateContext("acme");

        var contributor = new ConfigurationTenantResolveContributor();

        await contributor.ResolveAsync(context);

        context.TenantIdOrName.ShouldBe("acme");
    }

    [Fact]
    public async Task Should_Not_Resolve_If_Configuration_Is_Missing()
    {
        var context = CreateContext(null); // No tenant set

        var contributor = new ConfigurationTenantResolveContributor();

        await contributor.ResolveAsync(context);

        context.TenantIdOrName.ShouldBeNull();
    }

    // Reusable setup
    private static ITenantResolveContext CreateContext(string? tenantName)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IHostEnvironment>(new FakeHostEnvironment());
        var configBuilder = new ConfigurationBuilder();

        if (!string.IsNullOrWhiteSpace(tenantName))
        {
            configBuilder.AddInMemoryCollection(
            [
                new KeyValuePair<string, string>("MultiTenancy:Tenant", tenantName)
            ]);
        }

        services.AddSingleton<IConfiguration>(configBuilder.Build());

        var provider = services.BuildServiceProvider();
        return new FakeTenantResolveContext(provider);
    }

    // Fake context
    private class FakeTenantResolveContext : ITenantResolveContext
    {
        public IServiceProvider ServiceProvider { get; }

        public string? TenantIdOrName { get; set; }

        public bool Handled { get; set; }

        public FakeTenantResolveContext(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public bool HasResolvedTenantOrHost()
        {
            return Handled || !string.IsNullOrWhiteSpace(TenantIdOrName);
        }
    }

    private class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "TestApp";
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private class NullFileProvider : IFileProvider
    {
        public IDirectoryContents GetDirectoryContents(string subpath) => new NotFoundDirectoryContents();
        public IFileInfo GetFileInfo(string subpath) => new NotFoundFileInfo(subpath);
        public Microsoft.Extensions.Primitives.IChangeToken Watch(string filter) => NullChangeToken.Singleton;
    }
}
