using System.Drawing;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents something I've made that isn't a project: a drawing, a 3D render, or a piece of music.
/// </summary>
public sealed class Creation
{
    /// <summary>
    ///     Gets the unique identifier for the creation.
    /// </summary>
    /// <value>The unique identifier.</value>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets or sets the kind of creation.
    /// </summary>
    /// <value>The kind.</value>
    public CreationKind Kind { get; set; }

    /// <summary>
    ///     Gets a value indicating whether the creation is a piece of music.
    /// </summary>
    /// <value><see langword="true" /> if the creation is music; otherwise, <see langword="false" />.</value>
    public bool IsMusic
    {
        get => Kind == CreationKind.Music;
    }

    /// <summary>
    ///     Gets or sets the file name of the creation.
    /// </summary>
    /// <value>The file name.</value>
    /// <remarks>Superseded by <see cref="Media" />, which is the only thing read.</remarks>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the title of the creation.
    /// </summary>
    /// <value>The title.</value>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the description of the creation.
    /// </summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the creation was published.
    /// </summary>
    /// <value>The published date and time.</value>
    public DateTimeOffset PublishedAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time the creation was moved to the trash.
    /// </summary>
    /// <value>
    ///     The date and time the creation was trashed, or <see langword="null" /> if it is not trashed.
    /// </value>
    public DateTimeOffset? TrashedAt { get; set; }

    /// <summary>
    ///     Gets or sets the visibility of the creation.
    /// </summary>
    /// <value>The visibility.</value>
    public Visibility Visibility { get; set; } = Visibility.Published;

    /// <summary>
    ///     Gets or sets a value indicating whether this creation is a work in progress.
    /// </summary>
    /// <value><see langword="true" /> if this creation is a work in progress; otherwise, <see langword="false" />.</value>
    public bool IsWorkInProgress { get; set; }

    /// <summary>
    ///     Gets or sets a string that describes how this creation was made.
    /// </summary>
    /// <value>A string that describes how this creation was made.</value>
    public string? MadeWith { get; set; }

    /// <summary>
    ///     Gets or sets the pixel resolution of the creation's image.
    /// </summary>
    /// <value>The resolution, or <see langword="null" /> if the creation is music.</value>
    /// <remarks>Superseded by <see cref="Media" />. See <see cref="FileName" />.</remarks>
    public Size? Resolution { get; set; }

    /// <summary>
    ///     Gets or sets the duration of the creation's audio.
    /// </summary>
    /// <value>The duration, or <see langword="null" /> if the creation is not music.</value>
    /// <remarks>Superseded by <see cref="Media" />. See <see cref="FileName" />.</remarks>
    public TimeSpan? Duration { get; set; }
}
