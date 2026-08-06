using System.Diagnostics.CodeAnalysis;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service which can fetch multi-language code snippets.
/// </summary>
internal sealed class CodeSnippetService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CodeSnippetService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="AppDbContext" /> factory.</param>
    public CodeSnippetService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Returns all the languages which apply to the specified snippet.
    /// </summary>
    /// <param name="id">The ID of the snippet whose languages should be returned.</param>
    /// <returns>
    ///     A read-only view of the languages that apply to the snippet. This list may be empty if the snippet ID is invalid.
    /// </returns>
    public IReadOnlyList<string> GetLanguagesForSnippet(int id)
    {
        var languages = new HashSet<string>();
        using AppDbContext context = _dbContextFactory.CreateDbContext();

        foreach (CodeSnippet snippet in context.CodeSnippets.Where(s => s.Id == id))
        {
            languages.Add(snippet.Language);
        }

        return [.. languages];
    }

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
    public bool TryGetCodeSnippetForLanguage(int id, string language, [NotNullWhen(true)] out CodeSnippet? snippet)
    {
        if (language is null)
        {
            throw new ArgumentNullException(nameof(language));
        }

        using AppDbContext context = _dbContextFactory.CreateDbContext();
        IQueryable<CodeSnippet> snippets = context.CodeSnippets.Where(s => s.Id == id);
        snippet = snippets.FirstOrDefault(s => s.Language == language);
        return snippet is not null;
    }
}
