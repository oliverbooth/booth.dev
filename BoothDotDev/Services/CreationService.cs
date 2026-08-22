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
    ///     Gets a read-only view of the artwork items, excluding trashed ones.
    /// </summary>
    /// <param name="visibility">
    ///     The visibility of the artwork items to retrieve. A value of <see cref="Visibility.None" /> will retrieve every
    ///     non-trashed item regardless of visibility.
    /// </param>
    /// <returns>A read-only list of <see cref="ArtworkItem" /> objects.</returns>
    public IReadOnlyList<ArtworkItem> GetArtworkItems(Visibility visibility = Visibility.Published)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var items = dbContext.ArtworkItems.Where(a => a.TrashedAt == null);
        return
        [
            .. (visibility == Visibility.None ? items : items.Where(a => a.Visibility == visibility))
                .OrderByDescending(a => a.Published)
        ];
    }
}
