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
    /// Converts given <see cref="DateTime"/> to user's time zone.
    /// </summary>
    /// <param name="dateTime">DateTime to be normalized.</param>
    /// <returns>Converted DateTime</returns>
    DateTime Convert(DateTime dateTime);

    /// <summary>
    /// Converts given <see cref="DateTimeOffset"/> to user's time zone.
    /// </summary>
    /// <param name="dateTimeOffset">DateTimeOffset to be normalized.</param>
    /// <returns>Converted DateTimeOffset</returns>
    DateTimeOffset Convert(DateTimeOffset dateTimeOffset);
}
