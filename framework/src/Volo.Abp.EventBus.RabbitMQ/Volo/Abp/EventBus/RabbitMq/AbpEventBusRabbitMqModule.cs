using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.RabbitMQ;

namespace Volo.Abp.EventBus.RabbitMq;

[DependsOn(
    typeof(AbpEventBusModule),
    typeof(AbpRabbitMqModule))]
public class AbpEventBusRabbitMqModule : AbpModule
{
    protected HashSet<string> uint64QueueArguments =
    [
        "x-delivery-limit",
        "x-expires",
        "x-message-ttl",
        "x-max-length",
        "x-max-length-bytes",
        "x-quorum-initial-group-size",
        "x-quorum-target-group-size",
        "x-stream-filter-size-bytes",
        "x-stream-max-segment-size-bytes",
    ];
    protected HashSet<string> boolQueueArguments = ["x-single-active-consumer"];

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        Configure<AbpRabbitMqEventBusOptions>(configuration.GetSection("RabbitMQ:EventBus"));

        context.Services.Configure<AbpRabbitMqEventBusOptions>(options =>
        {
            ParseBoolQueueArguments(options);
            ParseIntegerQueueArguments(options);
        });
    }

    protected virtual void ParseBoolQueueArguments(AbpRabbitMqEventBusOptions options)
    {
        foreach (var argument in boolQueueArguments)
        {
            if (
                options.QueueArguments.TryGetValue(argument, out var value)
                && value is string stringValue
            )
            {
                options.QueueArguments[argument] = bool.Parse(stringValue);
            }
        }
    }

    protected virtual void ParseIntegerQueueArguments(AbpRabbitMqEventBusOptions options)
    {
        foreach (var argument in uint64QueueArguments)
        {
            if (
                options.QueueArguments.TryGetValue(argument, out var value)
                && value is string stringValue
            )
            {
                options.QueueArguments[argument] = int.Parse(stringValue);
            }
        }
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        context
            .ServiceProvider
            .GetRequiredService<IRabbitMqDistributedEventBus>()
            .Initialize();
    }
}
