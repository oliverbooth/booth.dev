using System.Globalization;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extension methods for <see cref="DateTimeOffset" /> and <see cref="DateOnly" />.
/// </summary>
internal static class DateTimeExtensions
{
    /// <param name="date">The <see cref="DateOnly" />.</param>
    extension(DateOnly date)
    {
        /// <summary>
        ///     Converts the <see cref="DateOnly" /> to a short string representation in the format "day month year"
        ///     (e.g., "1 jan 2024").
        /// </summary>
        /// <returns>A short string representation of the <see cref="DateOnly" />.</returns>
        public string ToShortString()
        {
            var month = date.ToString("MMM", CultureInfo.InvariantCulture).ToLowerInvariant();
            return $"{date.Day} {month} {date.Year}";
        }
    }

    /// <param name="dateTimeOffset">The <see cref="DateTimeOffset" />.</param>
    extension(DateTimeOffset dateTimeOffset)
    {
        /// <summary>
        ///     Converts the <see cref="DateTimeOffset" /> to a short string representation in the format "day month year"
        ///     (e.g., "1 jan 2024").
        /// </summary>
        /// <returns>A short string representation of the <see cref="DateTimeOffset" />.</returns>
        public string ToShortString()
        {
            var month = dateTimeOffset.ToString("MMM", CultureInfo.InvariantCulture).ToLowerInvariant();
            return $"{dateTimeOffset.Day} {month} {dateTimeOffset.Year}";
        }
    }
}
