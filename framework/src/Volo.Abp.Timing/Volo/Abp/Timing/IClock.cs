using System;

namespace Volo.Abp.Timing;

public interface IClock
{
    /// <summary>
    /// Gets Now.
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Gets kind.
    /// </summary>
    DateTimeKind Kind { get; }

    /// <summary>
    /// Is that provider supports multiple time zone.
    /// </summary>
    bool SupportsMultipleTimezone { get; }

    /// <summary>
    /// Normalizes given <see cref="DateTime"/>.
    /// </summary>
    /// <param name="dateTime">DateTime to be normalized.</param>
    /// <returns>Normalized DateTime</returns>
    DateTime Normalize(DateTime dateTime);

    /// <summary>
    /// Converts given UTC <see cref="DateTime"/> to user's time zone.
    /// </summary>
    /// <param name="dateTime">DateTime to be normalized.</param>
    /// <returns>Converted DateTime</returns>
    DateTime ConvertTo(DateTime dateTime);

    /// <summary>
    /// Converts given <see cref="DateTimeOffset"/> to user's time zone.
    /// </summary>
    /// <param name="dateTimeOffset">DateTimeOffset to be normalized.</param>
    /// <returns>Converted DateTimeOffset</returns>
    DateTimeOffset ConvertTo(DateTimeOffset dateTimeOffset);

    /// <summary>
    /// Converts given user's <see cref="DateTime"/> to UTC or not.
    /// </summary>
    /// <param name="dateTime">DateTime to be normalized.</param>
    /// <returns>Converted DateTime</returns>
    DateTime ConvertFrom(DateTime dateTime);
}
