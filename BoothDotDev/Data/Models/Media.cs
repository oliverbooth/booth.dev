namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a file (an image, a video, or audio) that belongs to a project or a creation.
/// </summary>
/// <remarks>
///     A file belongs to exactly one of <see cref="Project" /> or <see cref="Creation" />, and lives in that owner's folder on
///     the CDN. What kind of file it is comes from its extension.
/// </remarks>
public sealed class Media
{
    /// <summary>
    ///     Gets the unique identifier for the file.
    /// </summary>
    /// <value>The unique identifier.</value>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets or sets the ID of the project the file belongs to.
    /// </summary>
    /// <value>The project ID, or <see langword="null" /> if the file belongs to a creation.</value>
    public Guid? ProjectId { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the creation the file belongs to.
    /// </summary>
    /// <value>The creation ID, or <see langword="null" /> if the file belongs to a project.</value>
    public Guid? CreationId { get; set; }

    /// <summary>
    ///     Gets or sets the bare name of the file in its owner's CDN folder.
    /// </summary>
    /// <value>The file name.</value>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the position of the file among its owner's files, counting from zero.
    /// </summary>
    /// <value>The position.</value>
    public int Position { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the file is its owner's cover, the image shown on cards.
    /// </summary>
    /// <value><see langword="true" /> if the file is the cover; otherwise, <see langword="false" />.</value>
    /// <remarks>An owner has at most one cover, and only an image can be one.</remarks>
    public bool IsCover { get; set; }

    /// <summary>
    ///     Gets or sets the alternative text describing the file.
    /// </summary>
    /// <value>The alternative text, or <see langword="null" /> if it has none.</value>
    public string? Alt { get; set; }

    /// <summary>
    ///     Gets or sets the width of the file, in pixels.
    /// </summary>
    /// <value>The width, or <see langword="null" /> if the file is not an image.</value>
    public int? Width { get; set; }

    /// <summary>
    ///     Gets or sets the height of the file, in pixels.
    /// </summary>
    /// <value>The height, or <see langword="null" /> if the file is not an image.</value>
    public int? Height { get; set; }

    /// <summary>
    ///     Gets or sets the length of the file's audio.
    /// </summary>
    /// <value>The duration, or <see langword="null" /> if the file is not audio.</value>
    public TimeSpan? Duration { get; set; }
}
