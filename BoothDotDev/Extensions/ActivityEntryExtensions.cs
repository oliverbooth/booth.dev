using BoothDotDev.Data;
using BoothDotDev.Data.Models;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extensions for <see cref="ActivityEntry" />.
/// </summary>
public static class ActivityEntryExtensions
{
    /// <param name="entry">The <see cref="ActivityEntry" />.</param>
    extension(ActivityEntry entry)
    {
        /// <summary>
        ///     Gets the <see cref="PaletteHue" /> for this entry's <see cref="ActivityEntry.Category" />.
        /// </summary>
        /// <returns>The hue corresponding to this entry's content type.</returns>
        /// <remarks>
        ///     Category values come from <see cref="ActivityEntryFactory" /> and are not otherwise validated, so an
        ///     unrecognised value falls back to <see cref="PaletteHue.Brand" /> rather than throwing.
        /// </remarks>
        public PaletteHue ToHue()
        {
            return entry.Category switch
            {
                "blog" => PaletteHue.Brand,
                "tutorial" => PaletteHue.Mint,
                "devlog" => PaletteHue.Sky,
                "challenge" => PaletteHue.Pink,
                "note" => PaletteHue.Sun,
                _ => PaletteHue.Brand
            };
        }
    }
}
