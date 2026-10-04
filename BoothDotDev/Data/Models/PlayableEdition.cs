namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents an additional edition of a <see cref="Playable" /> that has also been played, such as a remaster.
/// </summary>
public sealed class PlayableEdition
{
    /// <summary>
    ///     Gets or sets the ID of the edition.
    /// </summary>
    /// <value>The ID of the edition.</value>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the IGDB slug of the edition.
    /// </summary>
    /// <value>The IGDB slug of the edition, or <see langword="null" /> if it has no IGDB entry of its own.</value>
    public string? IgdbSlug { get; set; }

    /// <summary>
    ///     Gets or sets the label of the edition.
    /// </summary>
    /// <value>The label, such as <c>Director's Cut</c>.</value>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the platforms the edition has been played on.
    /// </summary>
    /// <value>The platforms, in display order.</value>
    public List<GamePlatform> Platforms { get; set; } = [];

    /// <summary>
    ///     Gets or sets the ID of the game the edition belongs to.
    /// </summary>
    /// <value>The ID of the game.</value>
    public Guid PlayableId { get; set; }

    /// <summary>
    ///     Gets or sets the position of the edition among the game's other editions.
    /// </summary>
    /// <value>The zero-based position.</value>
    public int Position { get; set; }
}
