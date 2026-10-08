using BoothDotDev.Data;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extensions for <see cref="WatchableKind" />.
/// </summary>
public static class WatchableKindExtensions
{
    /// <param name="kind">The <see cref="WatchableKind" />.</param>
    extension(WatchableKind kind)
    {
        /// <summary>
        ///     Gets the <see cref="PaletteHue" /> a watchable of this kind is coloured with.
        /// </summary>
        /// <returns>The <see cref="PaletteHue" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="kind" /> is not a recognised <see cref="WatchableKind" />.
        /// </exception>
        public PaletteHue ToHue()
        {
            return kind switch
            {
                WatchableKind.Movie => PaletteHue.Bubblegum,
                WatchableKind.Show => PaletteHue.Grape,
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }
    }
}
