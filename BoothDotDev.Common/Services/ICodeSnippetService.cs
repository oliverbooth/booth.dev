using System.Diagnostics.CodeAnalysis;
using BoothDotDev.Common.Data.Web;

namespace BoothDotDev.Common.Services;

/// <summary>
///     Represents a service which can fetch multi-language code snippets.
/// </summary>
public interface ICodeSnippetService
{
    /// <summary>
    ///     Returns all the languages which apply to the specified snippet.
    /// </summary>
    /// <param name="id">The ID of the snippet whose languages should be returned.</param>
    /// <returns>
    ///     A read-only view of the languages that apply to the snippet. This list may be empty if the snippet ID is invalid.
    /// </returns>
    IReadOnlyList<string> GetLanguagesForSnippet(int id);

    /// <summary>
    ///     Attempts to find a code snippet by the specified ID, in the specified language.
    /// </summary>
    /// <param name="id">The ID of the snippet to search for.</param>
    /// <param name="language">The language to search for.</param>
    /// <param name="snippet">
    ///     When this method returns, contains the code snippet matching the specified criteria, if such a snippet was found;
    ///     otherwise, <see langword="null" />.
    /// </param>
    /// <returns><see langword="true" /> if the snippet was found; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="language" /> is <see langword="null" />.</exception>
    bool TryGetCodeSnippetForLanguage(int id, string language, [NotNullWhen(true)] out ICodeSnippet? snippet);
}
