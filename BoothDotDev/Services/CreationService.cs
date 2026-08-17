using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service responsible for managing creative content (artwork, music, etc.).
/// </summary>
public sealed class CreationService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CreationService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The database context factory.</param>
    public CreationService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Gets a read-only view of the published artwork items.
    /// </summary>
    /// <param name="visibility">
    ///     The visibility of the artwork items to retrieve. A value of <see cref="Visibility.None" /> will retrieve every item
    ///     regardless of visibility.
    /// </param>
    /// <returns>A read-only list of <see cref="ArtworkItem" /> objects.</returns>
    public IReadOnlyList<ArtworkItem> GetArtworkItems(Visibility visibility = Visibility.Published)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return visibility == Visibility.None
            ? [.. dbContext.ArtworkItems.OrderByDescending(a => a.Published)]
            : [.. dbContext.ArtworkItems.Where(a => a.Visibility == visibility).OrderByDescending(a => a.Published)];
    }
}
