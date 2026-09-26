using BoothDotDev.Data;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extensions for <see cref="PaletteHue" />.
/// </summary>
public static class PaletteHueExtensions
{
    private static readonly PaletteHue[] Hues = Enum.GetValues<PaletteHue>();

    // enum order puts brand and grape (both purple-ish) side by side; this order keeps neighbours visually distinct
    private static readonly PaletteHue[] PositionOrder =
    [
        PaletteHue.Brand, PaletteHue.Mint, PaletteHue.Sun, PaletteHue.Bubblegum, PaletteHue.Sky, PaletteHue.Tangerine, PaletteHue.Grape
    ];

    /// <summary>
    ///     Deterministically derives a <see cref="PaletteHue" /> from an identifier, for entities that have not been
    ///     assigned an explicit colour.
    /// </summary>
    /// <param name="id">The identifier to hash.</param>
    /// <returns>A <see cref="PaletteHue" /> that is stable for the given <paramref name="id" />.</returns>
    /// <remarks>Uses FNV-1a rather than <see cref="Guid.GetHashCode" /> so the result is stable across processes.</remarks>
    public static PaletteHue HashFrom(Guid id)
    {
        var hash = 2166136261u;
        foreach (var b in id.ToByteArray())
        {
            hash = (hash ^ b) * 16777619u;
        }

        return Hues[hash % (uint)Hues.Length];
    }

    /// <summary>
    ///     Derives a <see cref="PaletteHue" /> from an item's position in a list, for entities that have not been
    ///     assigned an explicit colour.
    /// </summary>
    /// <param name="index">The zero-based position of the item.</param>
    /// <returns>A <see cref="PaletteHue" />, cycling so that adjacent items are always different.</returns>
    public static PaletteHue FromPosition(int index)
    {
        return PositionOrder[index % PositionOrder.Length];
    }

    /// <param name="hue">The <see cref="PaletteHue" />.</param>
    extension(PaletteHue hue)
    {
        /// <summary>
        ///     Gets the solid colour of the <see cref="PaletteHue" />, for places that can't read the site's CSS tokens, such as
        ///     generated images and Discord embeds.
        /// </summary>
        /// <returns>The colour as a 24-bit sRGB integer.</returns>
        public int ToRgb()
        {
            return hue switch
            {
                PaletteHue.Grape => 0xAB6AEA,
                PaletteHue.Bubblegum => 0xE867C3,
                PaletteHue.Tangerine => 0xFF7041,
                PaletteHue.Sun => 0xF2CF3B,
                PaletteHue.Mint => 0x00CD94,
                PaletteHue.Sky => 0x2EB1EF,
                _ => 0x6161CD
            };
        }

        /// <summary>
        ///     Gets the <c>data-hue</c> attribute value corresponding to the <see cref="PaletteHue" />.
        /// </summary>
        /// <returns>The attribute value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="hue" /> is not a recognised <see cref="PaletteHue" />.
        /// </exception>
        public string ToDataHue()
        {
            return hue switch
            {
                PaletteHue.Brand => "brand",
                PaletteHue.Grape => "grape",
                PaletteHue.Bubblegum => "bubblegum",
                PaletteHue.Tangerine => "tangerine",
                PaletteHue.Sun => "sun",
                PaletteHue.Mint => "mint",
                PaletteHue.Sky => "sky",
                _ => throw new ArgumentOutOfRangeException(nameof(hue), hue, null)
            };
        }

        /// <summary>
        ///     Gets the CSS modifier class corresponding to the <see cref="PaletteHue" />, for use alongside
        ///     <c>badge</c>.
        /// </summary>
        /// <returns>The CSS modifier class name, or an empty string if <c>badge</c> alone already renders this hue.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="hue" /> is not a recognised <see cref="PaletteHue" />.
        /// </exception>
        public string ToBadgeClass()
        {
            return hue switch
            {
                PaletteHue.Brand => "badge-brand",
                PaletteHue.Grape => "badge-grape",
                PaletteHue.Bubblegum => "badge-bubblegum",
                PaletteHue.Tangerine => "badge-tangerine",
                PaletteHue.Sun => "badge-sun",
                PaletteHue.Mint => "",
                PaletteHue.Sky => "badge-sky",
                _ => throw new ArgumentOutOfRangeException(nameof(hue), hue, null)
            };
        }
    }
}
