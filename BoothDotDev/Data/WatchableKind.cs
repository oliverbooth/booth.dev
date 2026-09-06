using NpgsqlTypes;

namespace BoothDotDev.Data;

/// <summary>
///     Represents the kind of a watchable item.
/// </summary>
public enum WatchableKind
{
    /// <summary>
    ///     The item is a movie.
    /// </summary>
    [PgName("movie")]
    Movie,

    /// <summary>
    ///     The item is a TV show.
    /// </summary>
    [PgName("show")]
    Show
}
