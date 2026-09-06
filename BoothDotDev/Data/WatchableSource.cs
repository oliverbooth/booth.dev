using NpgsqlTypes;

namespace BoothDotDev.Data;

/// <summary>
///     Represents where a watchlist entry came from.
/// </summary>
public enum WatchableSource
{
    /// <summary>
    ///     The entry was added by hand.
    /// </summary>
    [PgName("manual")] Manual,

    /// <summary>
    ///     The entry was synced from Trakt.
    /// </summary>
    [PgName("trakt")] Trakt
}
