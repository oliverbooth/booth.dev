using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service which can perform programming language lookup.
/// </summary>
internal sealed class ProgrammingLanguageService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProgrammingLanguageService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="AppDbContext" /> factory.</param>
    public ProgrammingLanguageService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Returns the human-readable name of a language.
    /// </summary>
    /// <param name="alias">The alias of the language.</param>
    /// <returns>The human-readable name, or <paramref name="alias" /> if the name could not be found.</returns>
    public string GetLanguageName(string alias)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        ProgrammingLanguage? language = context.ProgrammingLanguages.FirstOrDefault(l => l.Key == alias);
        return language?.Name ?? alias;
    }
}
