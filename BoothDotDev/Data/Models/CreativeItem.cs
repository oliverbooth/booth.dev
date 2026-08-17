namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a creative item.
/// </summary>
public abstract class CreativeItem
{
    /// <summary>
    ///     Gets the unique identifier for the creative item.
    /// </summary>
    /// <value>The unique identifier.</value>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets or sets the file name of the creative item.
    /// </summary>
    /// <value>The file name.</value>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the title of the creative item.
    /// </summary>
    /// <value>The title.</value>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the description of the creative item.
    /// </summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the creative item was published.
    /// </summary>
    /// <value>The published date and time.</value>
    public DateTimeOffset Published { get; set; }

    /// <summary>
    ///     Gets or sets the visibility of the creative item.
    /// </summary>
    /// <value>The visibility.</value>
    public Visibility Visibility { get; set; } = Visibility.Published;
    
    /// <summary>
    ///     Gets or sets a value indicating whether this creative item is a work in progress.
    /// </summary>
    /// <value><see langword="true"/> if this creative item is a work in progress; otherwise, <see langword="false"/>.</value>
    public bool IsWorkInProgress { get; set; }

    /// <summary>
    ///     Gets or sets a string that describes how this creative item was made.
    /// </summary>
    /// <value>A string that describes how this creative item was made.</value>
    public string? MadeWith { get; set; }
}
