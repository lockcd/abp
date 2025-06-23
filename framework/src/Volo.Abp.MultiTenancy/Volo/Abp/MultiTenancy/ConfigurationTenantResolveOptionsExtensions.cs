using System;
using System.Collections.Generic;
using System.Text;

namespace Volo.Abp.MultiTenancy;

public static class ConfigurationTenantResolveOptionsExtensions
{
    public static void AddConfigurationTenantResolver(
        this AbpTenantResolveOptions options)
    {
        options.TenantResolvers.InsertAfter(
            r => r is CurrentUserTenantResolveContributor,
            new ConfigurationTenantResolveContributor()
        );
    }
}
