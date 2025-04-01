using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Middleware;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Volo.Abp.Users;

namespace Microsoft.AspNetCore.Timing;

public class AbpTimeZoneMiddleware : AbpMiddlewareBase, ITransientDependency
{
    public async override Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.RequestServices.GetRequiredService<IClock>().SupportsMultipleTimezone)
        {
            await next(context);
            return;
        }

        string? timezone = null;

        if (!context.RequestServices.GetRequiredService<ICurrentUser>().IsAuthenticated)
        {
            timezone = await GetTimezoneFromRequestAsync(context);
        }

        if (timezone.IsNullOrWhiteSpace())
        {
            var settingProvider = context.RequestServices.GetRequiredService<ISettingProvider>();
            timezone = await settingProvider.GetOrNullAsync(TimingSettingNames.TimeZone);
        }

        if (timezone.IsNullOrWhiteSpace())
        {
            timezone = context.RequestServices.GetRequiredService<ITimezoneProvider>().GetCurrentIanaTimezoneName();
        }

        var currentTimezoneProvider = context.RequestServices.GetRequiredService<ICurrentTimezoneProvider>();
        using (currentTimezoneProvider.Change(timezone))
        {
            await next(context);
        }
    }

    protected virtual async Task<string? > GetTimezoneFromRequestAsync(HttpContext context)
    {
        var timeZoneSources = new Func<HttpContext, Task<string?>>[]
        {
            ctx => Task.FromResult(ctx.Request.Headers[TimeZoneConsts.DefaultTimeZoneKey].FirstOrDefault()),
            ctx => Task.FromResult<string?>(ctx.Request.Query[TimeZoneConsts.DefaultTimeZoneKey].ToString()),
            async ctx => ctx.Request.HasFormContentType
                ? (await ctx.Request.ReadFormAsync())[TimeZoneConsts.DefaultTimeZoneKey].ToString()
                : null,
            ctx => Task.FromResult(ctx.Request.Cookies[TimeZoneConsts.DefaultTimeZoneKey]?.ToString()),
        };

        foreach (var source in timeZoneSources)
        {
            var timezone = await source(context);
            if (!string.IsNullOrEmpty(timezone))
            {
                return timezone;
            }
        }

        return null;
    }
}
