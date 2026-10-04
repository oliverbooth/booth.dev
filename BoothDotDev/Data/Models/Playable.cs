namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a game on the game list.
/// </summary>
public sealed class Playable
{
    /// <summary>
    ///     Gets or sets the ID of the game.
    /// </summary>
    /// <value>The ID of the game.</value>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the IGDB slug of the game.
    /// </summary>
    /// <value>The IGDB slug of the game, or <see langword="null" /> if it isn't linked to IGDB.</value>
    public string? IgdbSlug { get; set; }

    /// <summary>
    ///     Gets or sets the platforms the game has been played on.
    /// </summary>
    /// <value>The platforms, in display order, or an empty list if none were recorded.</value>
    public List<GamePlatform> Platforms { get; set; } = [];

    /// <summary>
    ///     Gets or sets the state of the game.
    /// </summary>
    /// <value>The state of the game.</value>
    public PlayableState State { get; set; }

    /// <summary>
    ///     Gets or sets the title of the game.
    /// </summary>
    /// <value>The title of the game.</value>
    public string Title { get; set; } = string.Empty;
}
