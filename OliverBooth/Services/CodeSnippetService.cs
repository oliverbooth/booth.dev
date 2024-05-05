using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Common.Data.Web;
using OliverBooth.Common.Services;
using OliverBooth.Data.Web;

namespace OliverBooth.Services;

/// <inheritdoc />
internal sealed class CodeSnippetService : ICodeSnippetService
{
    private readonly IDbContextFactory<WebContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CodeSnippetService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="WebContext" /> factory.</param>
    public CodeSnippetService(IDbContextFactory<WebContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetLanguagesForSnippet(int id)
    {
        var languages = new HashSet<string>();
        using WebContext context = _dbContextFactory.CreateDbContext();

        foreach (CodeSnippet snippet in context.CodeSnippets.Where(s => s.Id == id))
        {
            languages.Add(snippet.Language);
        }

        return languages.ToArray();
    }

    /// <inheritdoc />
    public bool TryGetCodeSnippetForLanguage(int id, string language, [NotNullWhen(true)] out ICodeSnippet? snippet)
    {
        if (language is null)
        {
            throw new ArgumentNullException(nameof(language));
        }

        using WebContext context = _dbContextFactory.CreateDbContext();
        IQueryable<CodeSnippet> snippets = context.CodeSnippets.Where(s => s.Id == id);
        snippet = snippets.FirstOrDefault(s => s.Language == language);
        return snippet is not null;
    }
}
