using System;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Timing;

public class Clock : IClock, ITransientDependency
{
    protected AbpClockOptions Options { get; }
    protected ICurrentTimezoneProvider CurrentTimezoneProvider { get; }
    protected ITimezoneProvider TimezoneProvider { get; }

    public Clock(
        IOptions<AbpClockOptions> options,
        ICurrentTimezoneProvider currentTimezoneProvider,
        ITimezoneProvider timezoneProvider)
    {
        CurrentTimezoneProvider = currentTimezoneProvider;
        TimezoneProvider = timezoneProvider;
        Options = options.Value;
    }

    public virtual DateTime Now => Options.Kind == DateTimeKind.Utc ? DateTime.UtcNow : DateTime.Now;

    public virtual DateTimeKind Kind => Options.Kind;

    public virtual bool SupportsMultipleTimezone => Options.Kind == DateTimeKind.Utc;

    public virtual DateTime Normalize(DateTime dateTime)
    {
        if (Kind == DateTimeKind.Unspecified || Kind == dateTime.Kind)
        {
            return dateTime;
        }

        if (Kind == DateTimeKind.Local && dateTime.Kind == DateTimeKind.Utc)
        {
            return dateTime.ToLocalTime();
        }

        if (Kind == DateTimeKind.Utc && dateTime.Kind == DateTimeKind.Local)
        {
            return dateTime.ToUniversalTime();
        }

        return DateTime.SpecifyKind(dateTime, Kind);
    }

    public virtual DateTime Convert(DateTime dateTime)
    {
        if (!SupportsMultipleTimezone ||
            dateTime.Kind != DateTimeKind.Utc ||
            CurrentTimezoneProvider.TimeZone.IsNullOrWhiteSpace())
        {
            return dateTime;
        }

        var timezoneInfo = TimezoneProvider.GetTimeZoneInfo(CurrentTimezoneProvider.TimeZone);
        return TimeZoneInfo.ConvertTime(dateTime, timezoneInfo);
    }
}
