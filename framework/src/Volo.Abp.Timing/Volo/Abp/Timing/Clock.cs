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

    /// <summary>
    /// Normalizes given <see cref="DateTime"/>.
    /// </summary>
    /// <param name="dateTime">DateTime to be normalized.</param>
    /// <returns>Normalized DateTime</returns>
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

    /// <summary>
    /// Converts given UTC <see cref="DateTime"/> to user's time zone.
    /// </summary>
    /// <param name="dateTime">DateTime to be normalized.</param>
    /// <returns>Converted DateTime</returns>
    public virtual DateTime ConvertTo(DateTime dateTime)
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

    /// <summary>
    /// Converts given <see cref="DateTimeOffset"/> to user's time zone.
    /// </summary>
    /// <param name="dateTimeOffset">DateTimeOffset to be normalized.</param>
    /// <returns>Converted DateTimeOffset</returns>
    public virtual DateTimeOffset ConvertTo(DateTimeOffset dateTimeOffset)
    {
        if (!SupportsMultipleTimezone ||
            CurrentTimezoneProvider.TimeZone.IsNullOrWhiteSpace())
        {
            return dateTimeOffset;
        }

        var timezoneInfo = TimezoneProvider.GetTimeZoneInfo(CurrentTimezoneProvider.TimeZone);
        return TimeZoneInfo.ConvertTime(dateTimeOffset, timezoneInfo);
    }

    /// <summary>
    /// Converts given user's <see cref="DateTime"/> to UTC or not.
    /// </summary>
    /// <param name="dateTime">DateTime to be normalized.</param>
    /// <returns>Converted DateTime</returns>
    public DateTime ConvertFrom(DateTime dateTime)
    {
        if (!SupportsMultipleTimezone ||
            dateTime.Kind == DateTimeKind.Utc ||
            CurrentTimezoneProvider.TimeZone.IsNullOrWhiteSpace())
        {
            return dateTime;
        }

        var timezoneInfo = TimezoneProvider.GetTimeZoneInfo(CurrentTimezoneProvider.TimeZone);
        dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(dateTime, timezoneInfo);
    }
}
