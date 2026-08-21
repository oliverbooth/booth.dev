namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a note.
/// </summary>
public sealed class Note
{
    /// <summary>
    ///     Gets the unique identifier for the note.
    /// </summary>
    /// <value>A <see cref="Guid" /> representing the unique identifier for the note.</value>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets or sets the title of the note.
    /// </summary>
    /// <value>The title of the note.</value>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the content of the note.
    /// </summary>
    /// <value>The content of the note.</value>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the font style of the note.
    /// </summary>
    /// <value>The font style of the note.</value>
    public FontStyle FontStyle { get; set; } = FontStyle.Serif;

    /// <summary>
    ///     Gets the date and time when the note was published.
    /// </summary>
    /// <value>A <see cref="DateTimeOffset" /> representing the date and time when the note was published.</value>
    public DateTimeOffset Published { get; internal set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets the date and time when the note was last updated.
    /// </summary>
    /// <value>A <see cref="DateTimeOffset" /> representing the date and time when the note was last updated.</value>
    public DateTimeOffset? Updated { get; set; }

    /// <summary>
    ///     Gets or sets the visibility of the note.
    /// </summary>
    /// <value>The visibility of the note.</value>
    public Visibility Visibility { get; set; } = Visibility.Published;

    /// <summary>
    ///     Gets or sets the date and time the note was trashed.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTimeOffset" /> representing when the note was trashed, or <see langword="null" /> if it
    ///     isn't trashed.
    /// </value>
    public DateTimeOffset? TrashedAt { get; set; }
}
