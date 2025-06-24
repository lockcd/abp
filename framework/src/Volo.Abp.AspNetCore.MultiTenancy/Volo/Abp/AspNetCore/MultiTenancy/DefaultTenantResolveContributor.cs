using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Volo.Abp.MultiTenancy;

namespace Volo.Abp.AspNetCore.MultiTenancy;

public class DefaultTenantResolveContributor : TenantResolveContributorBase
{
    public const string ContributorName = "Default";

    public override string Name => ContributorName;

    public override Task ResolveAsync(ITenantResolveContext context)
    {
        var defaultTenant = context.GetAbpAspNetCoreMultiTenancyOptions().DefaultTenant;
        if (!string.IsNullOrWhiteSpace(defaultTenant))
        {
            context.TenantIdOrName = defaultTenant;
            context.Handled = true;
        }

        return Task.CompletedTask;
    }
}
