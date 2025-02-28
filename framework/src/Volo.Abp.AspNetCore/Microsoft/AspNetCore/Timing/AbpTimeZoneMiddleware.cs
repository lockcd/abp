using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Middleware;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using Volo.Abp.Timing;

namespace Microsoft.AspNetCore.Timing;

public class AbpTimeZoneMiddleware : AbpMiddlewareBase, ITransientDependency
{
    public async override Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var settingProvider = context.RequestServices.GetRequiredService<ISettingProvider>();
        var timezone = await settingProvider.GetOrNullAsync(TimingSettingNames.TimeZone);
        if (timezone.IsNullOrEmpty())
        {
            await next(context);
            return;
        }

        var currentTimezoneProvider = context.RequestServices.GetRequiredService<ICurrentTimezoneProvider>();
        using (currentTimezoneProvider.Change(timezone))
        {
            await next(context);
        }
    }
}
