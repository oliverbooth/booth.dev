using Microsoft.EntityFrameworkCore;
using OliverBooth.Data.Web;

namespace OliverBooth.Services;

/// <summary>
///     Represents a service which can perform programming language lookup.
/// </summary>
public interface IProgrammingLanguageService
{
    /// <summary>
    ///     Returns the human-readable name of a language.
    /// </summary>
    /// <param name="alias">The alias of the language.</param>
    /// <returns>The human-readable name, or <paramref name="alias" /> if the name could not be found.</returns>
    string GetLanguageName(string alias);
}

/// <inheritdoc />
internal sealed class ProgrammingLanguageService : IProgrammingLanguageService
{
    private readonly IDbContextFactory<WebContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProgrammingLanguageService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="WebContext" /> factory.</param>
    public ProgrammingLanguageService(IDbContextFactory<WebContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public string GetLanguageName(string alias)
    {
        using WebContext context = _dbContextFactory.CreateDbContext();
        ProgrammingLanguage? language = context.ProgrammingLanguages.FirstOrDefault(l => l.Key == alias);
        return language?.Name ?? alias;
    }
}
