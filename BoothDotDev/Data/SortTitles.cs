using System.Text.RegularExpressions;

namespace BoothDotDev.Data;

/// <summary>
///     Provides the library-style filing rules for titles on the reading list, watchlist and game list.
/// </summary>
public static partial class SortTitles
{
    [GeneratedRegex(@"^(?:the|an?)\s+(?=\S)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex LeadingArticle();

    /// <summary>
    ///     Gets the name a title files under by default: the title without a leading "The", "A" or "An".
    /// </summary>
    /// <param name="title">The title.</param>
    /// <returns>The title with any leading article removed, such as <c>Simpsons</c> for <c>The Simpsons</c>.</returns>
    public static string Default(string title)
    {
        return LeadingArticle().Replace(title.Trim(), string.Empty, 1);
    }

    /// <summary>
    ///     Gets the sort title to store for a title, if it needs one at all.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <returns>
    ///     The default sort title, or <see langword="null" /> if it's the same as <paramref name="title" />, in which
    ///     case nothing needs overriding.
    /// </returns>
    public static string? Recompute(string title)
    {
        var sortTitle = Default(title);
        return sortTitle == title.Trim() ? null : sortTitle;
    }

    /// <summary>
    ///     Normalizes a submitted sort title.
    /// </summary>
    /// <param name="sortTitle">The submitted sort title.</param>
    /// <returns>The trimmed sort title, or <see langword="null" /> if it's blank.</returns>
    public static string? Normalize(string? sortTitle)
    {
        return string.IsNullOrWhiteSpace(sortTitle) ? null : sortTitle.Trim();
    }
}