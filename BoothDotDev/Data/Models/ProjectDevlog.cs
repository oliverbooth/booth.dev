namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a devlog entry for a project.
/// </summary>
public sealed class ProjectDevlog : IMarkdownBody
{
    /// <summary>
    ///     Gets or sets the body content of the devlog entry.
    /// </summary>
    /// <value>The body content of the devlog entry.</value>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether comments are enabled for the devlog entry.
    /// </summary>
    /// <value><see langword="true"/> if comments are enabled; otherwise, <see langword="false"/>.</value>
    public bool EnableComments { get; set; } = true;

    /// <summary>
    ///     Gets the unique identifier for the devlog entry.
    /// </summary>
    /// <value>The unique identifier for the devlog entry.</value>
    public Guid Id { get; internal set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets the unique identifier for the project to which this devlog entry belongs.
    /// </summary>
    /// <value>The unique identifier for the project.</value>
    public Guid ProjectId { get; internal set; }

    /// <summary>
    ///     Gets or sets the publication date and time of the devlog entry.
    /// </summary>
    /// <value>The publication date and time of the devlog entry.</value>
    public DateTimeOffset Published { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets the slug (URL-friendly identifier) for the devlog entry.
    /// </summary>
    /// <value>The slug (URL-friendly identifier) for the devlog entry.</value>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the title of the devlog entry.
    /// </summary>
    /// <value>The title of the devlog entry.</value>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the last updated date and time of the devlog entry.
    /// </summary>
    /// <value>The last updated date and time of the devlog entry.</value>
    public DateTimeOffset? Updated { get; set; }

    /// <summary>
    ///     Gets or sets the visibility status of the devlog entry.
    /// </summary>
    /// <value>The visibility status of the devlog entry.</value>
    public Visibility Visibility { get; set; } = Visibility.Published;
}
