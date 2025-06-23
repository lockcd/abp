using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Volo.Abp;

namespace Volo.Abp.MultiTenancy;

public class ConfigurationTenantResolveContributor : TenantResolveContributorBase
{
    public const string ContributorName = "Configuration";
    public override string Name => ContributorName;

    public override async Task ResolveAsync(ITenantResolveContext context)
    {
        var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
        var tenantIdOrName = configuration["MultiTenancy:Tenant"]; 

        if (!tenantIdOrName.IsNullOrEmpty())
        {
            context.TenantIdOrName = tenantIdOrName;
        }

        await Task.CompletedTask;
    }
}
