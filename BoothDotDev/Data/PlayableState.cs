using NpgsqlTypes;

namespace BoothDotDev.Data;

/// <summary>
///     Represents the state of a playable game.
/// </summary>
public enum PlayableState
{
    /// <summary>
    ///     The game has been played and finished.
    /// </summary>
    [PgName("played")] Played,

    /// <summary>
    ///     The game is currently in rotation.
    /// </summary>
    [PgName("playing")] Playing,

    /// <summary>
    ///     The game is on a future playlist.
    /// </summary>
    [PgName("plan_to_play")] PlanToPlay
}
