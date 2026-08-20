using BoothDotDev.Data;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extensions for <see cref="FontStyle" />.
/// </summary>
public static class FontStyleExtensions
{
    /// <param name="fontStyle">The <see cref="FontStyle" />.</param>
    extension(FontStyle fontStyle)
    {
        /// <summary>
        ///     Gets the CSS modifier class corresponding to the <see cref="FontStyle" />, for use alongside
        ///     <c>prose</c>.
        /// </summary>
        /// <returns>The CSS modifier class name.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="fontStyle" /> is not a recognised <see cref="FontStyle" />.
        /// </exception>
        /// <remarks>
        ///     The class names deliberately do not correspond to the enum member names, so they cannot be derived by
        ///     lowercasing <see cref="object.ToString" />: <see cref="FontStyle.SansSerif" /> maps to
        ///     <c>prose--sans</c>, not <c>prose--sansserif</c>.
        /// </remarks>
        public string ToProseClass()
        {
            return fontStyle switch
            {
                FontStyle.SansSerif => "prose--sans",
                FontStyle.Serif => "prose--serif",
                _ => throw new ArgumentOutOfRangeException(nameof(fontStyle), fontStyle, null)
            };
        }
    }
}
