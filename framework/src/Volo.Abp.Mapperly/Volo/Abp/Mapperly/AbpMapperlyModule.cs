using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Auditing;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.ObjectMapping;

namespace Volo.Abp.Mapperly;

[DependsOn(
    typeof(AbpObjectMappingModule),
    typeof(AbpObjectExtendingModule),
    typeof(AbpAuditingModule)
)]
public class AbpMapperlyModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddConventionalRegistrar(new AbpMapperlyConventionalRegistrar());
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // This is the temporary solution, We will remove it in when all apps are migrated to Mapperly.
        var abpApplication = context.Services.GetSingletonInstance<IAbpApplication>();
        var modules = abpApplication.Modules.ToList();
        var autoMapperModuleIndex = modules.FindIndex(x => x.Type.FullName!.Equals("Volo.Abp.AutoMapper.AbpAutoMapperModule", StringComparison.OrdinalIgnoreCase));
        var mapperlyModuleIndex = modules.FindIndex(x => x.Type == typeof(AbpMapperlyModule));
        if (mapperlyModuleIndex < autoMapperModuleIndex)
        {
            context.Services.AddMapperlyObjectMapper();
        }
    }
}
