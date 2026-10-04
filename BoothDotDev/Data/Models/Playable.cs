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
    ///     Gets the platforms the game has been played on across every edition.
    /// </summary>
    /// <value>The distinct platforms, in display order.</value>
    public IReadOnlyList<GamePlatform> AllPlatforms
    {
        get => Platforms.Concat(Editions.SelectMany(edition => edition.Platforms)).Distinct().Order().ToList();
    }

    /// <summary>
    ///     Gets or sets the additional editions of the game that have also been played.
    /// </summary>
    /// <value>The additional editions, in display order. The game's own fields describe the primary edition.</value>
    public List<PlayableEdition> Editions { get; set; } = [];

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
