﻿using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

namespace Volo.Abp.BackgroundWorkers;

[DependsOn(
    typeof(AbpThreadingModule),
    typeof(AbpDataModule)
)]
public class AbpBackgroundWorkersModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        if (context.Services.IsDataMigrationEnvironment())
        {
            Configure<AbpBackgroundWorkerOptions>(options =>
            {
                options.IsEnabled = false;
            });
        }
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var options = context.ServiceProvider.GetRequiredService<IOptions<AbpBackgroundWorkerOptions>>().Value;
        if (options.IsEnabled)
        {
            await context.ServiceProvider
                .GetRequiredService<IBackgroundWorkerManager>()
                .StartAsync();
        }
    }

    public override async Task OnApplicationShutdownAsync(ApplicationShutdownContext context)
    {
        var options = context.ServiceProvider.GetRequiredService<IOptions<AbpBackgroundWorkerOptions>>().Value;
        if (options.IsEnabled)
        {
            await context.ServiceProvider
                .GetRequiredService<IBackgroundWorkerManager>()
                .StopAsync();
        }
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        AsyncHelper.RunSync(() => OnApplicationInitializationAsync(context));
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        AsyncHelper.RunSync(() => OnApplicationShutdownAsync(context));
    }
}
