using BoothDotDev.Data;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extensions for <see cref="CreationKind" />.
/// </summary>
public static class CreationKindExtensions
{
    /// <param name="kind">The kind of creation.</param>
    extension(CreationKind kind)
    {
        /// <summary>
        ///     Gets the <see cref="PortfolioItemKind" /> a creation of the specified kind is listed as.
        /// </summary>
        /// <returns>The <see cref="PortfolioItemKind" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="kind" /> is not a defined <see cref="CreationKind" />.
        /// </exception>
        public PortfolioItemKind ToItemKind()
        {
            return kind switch
            {
                CreationKind.Drawing => PortfolioItemKind.Drawing,
                CreationKind.ThreeD => PortfolioItemKind.ThreeD,
                CreationKind.Music => PortfolioItemKind.Music,
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }

        /// <summary>
        ///     Gets the colour a creation of the specified kind is shown in.
        /// </summary>
        /// <returns>The <see cref="PaletteHue" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="kind" /> is not a defined <see cref="CreationKind" />.
        /// </exception>
        public PaletteHue ToHue()
        {
            return kind switch
            {
                CreationKind.Drawing => PaletteHue.Bubblegum,
                CreationKind.ThreeD => PaletteHue.Grape,
                CreationKind.Music => PaletteHue.Mint,
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }

        /// <summary>
        ///     Gets the short lowercase name a creation of the specified kind is labelled with.
        /// </summary>
        /// <returns>The label, such as <c>drawing</c> or <c>3d</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="kind" /> is not a defined <see cref="CreationKind" />.
        /// </exception>
        public string ToLabel()
        {
            return kind switch
            {
                CreationKind.Drawing => "drawing",
                CreationKind.ThreeD => "3d",
                CreationKind.Music => "music",
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }
    }
}
