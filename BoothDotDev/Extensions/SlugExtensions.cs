using System.Text;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extensions for <see cref="string" /> that turn text into URL slugs.
/// </summary>
public static class SlugExtensions
{
    /// <param name="text">The text to convert.</param>
    extension(string text)
    {
        /// <summary>
        ///     Converts text to a lowercase slug of letters, digits, and single dashes.
        /// </summary>
        /// <returns>The slug, or an empty string if <paramref name="text" /> has no letters or digits.</returns>
        public string ToSlug()
        {
            var builder = new StringBuilder();
            var lastWasDash = false;

            foreach (var ch in text.ToLowerInvariant())
            {
                if (ch is >= 'a' and <= 'z' or >= '0' and <= '9')
                {
                    builder.Append(ch);
                    lastWasDash = false;
                }
                else if ((char.IsWhiteSpace(ch) || ch is '-' or '_') && !lastWasDash)
                {
                    builder.Append('-');
                    lastWasDash = true;
                }
            }

            return builder.ToString().Trim('-');
        }
    }
}
