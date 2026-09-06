namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a movie or TV show on the watchlist.
/// </summary>
public sealed class Watchable
{
    /// <summary>
    ///     Gets or sets the ID of the item.
    /// </summary>
    /// <value>The ID of the item.</value>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the kind of the item.
    /// </summary>
    /// <value>The kind of the item.</value>
    public WatchableKind Kind { get; set; }

    /// <summary>
    ///     Gets or sets the state of the item.
    /// </summary>
    /// <value>The state of the item.</value>
    public WatchableState State { get; set; }

    /// <summary>
    ///     Gets or sets the title of the item.
    /// </summary>
    /// <value>The title of the item.</value>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets where this entry came from.
    /// </summary>
    /// <value>The source of the entry.</value>
    public WatchableSource Source { get; set; }

    /// <summary>
    ///     Gets or sets the Trakt ID of the item, if it was synced from Trakt.
    /// </summary>
    /// <value>The Trakt ID of the item, or <see langword="null" /> for a manually-added entry.</value>
    public int? TraktId { get; set; }
}
