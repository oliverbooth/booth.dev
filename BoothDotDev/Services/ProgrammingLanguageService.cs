using BoothDotDev.Common.Services;
using BoothDotDev.Data.Web;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

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
