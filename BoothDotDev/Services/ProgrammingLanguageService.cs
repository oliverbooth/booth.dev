using BoothDotDev.Common.Services;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <inheritdoc />
internal sealed class ProgrammingLanguageService : IProgrammingLanguageService
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

    /// <inheritdoc />
    public string GetLanguageName(string alias)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        ProgrammingLanguage? language = context.ProgrammingLanguages.FirstOrDefault(l => l.Key == alias);
        return language?.Name ?? alias;
    }
}
