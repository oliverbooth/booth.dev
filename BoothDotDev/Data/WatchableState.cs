using NpgsqlTypes;

namespace BoothDotDev.Data;

/// <summary>
///     Represents the state of a watchable item.
/// </summary>
public enum WatchableState
{
    /// <summary>
    ///     The item has been watched and finished.
    /// </summary>
    [PgName("watched")]
    Watched,

    /// <summary>
    ///     The item is currently being watched.
    /// </summary>
    [PgName("watching")]
    Watching,

    /// <summary>
    ///     The item is on a future watchlist.
    /// </summary>
    [PgName("plan_to_watch")]
    PlanToWatch
}
