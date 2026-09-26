using BoothDotDev.Data;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extensions for <see cref="Difficulty" />.
/// </summary>
public static class DifficultyExtensions
{
    /// <param name="difficulty">The <see cref="Difficulty" />.</param>
    extension(Difficulty difficulty)
    {
        /// <summary>
        ///     Gets the <see cref="PaletteHue" /> a challenge of this difficulty is coloured with.
        /// </summary>
        /// <returns>The <see cref="PaletteHue" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="difficulty" /> is not a recognised <see cref="Difficulty" />.
        /// </exception>
        public PaletteHue ToHue()
        {
            return difficulty switch
            {
                Difficulty.Easy => PaletteHue.Mint,
                Difficulty.Intermediate => PaletteHue.Sun,
                Difficulty.Hard => PaletteHue.Tangerine,
                Difficulty.Insane => PaletteHue.Pink,
                _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
            };
        }
    }
}
