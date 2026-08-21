using System.Globalization;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extension methods for <see cref="DateTimeOffset" />.
/// </summary>
internal static class DateTimeExtensions
{
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
