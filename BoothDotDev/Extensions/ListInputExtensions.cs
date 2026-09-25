namespace BoothDotDev.Extensions;

/// <summary>
///     Extensions for <see cref="string" /> that read a list typed into a single form field.
/// </summary>
public static class ListInputExtensions
{
    /// <param name="text">The comma-separated text.</param>
    extension(string text)
    {
        /// <summary>
        ///     Splits comma-separated text into its trimmed, non-empty entries, dropping repeats regardless of case.
        /// </summary>
        /// <returns>The entries, in the order they were written.</returns>
        public List<string> SplitList()
        {
            return
            [
                .. text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
            ];
        }
    }
}
