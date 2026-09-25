using BoothDotDev.Data;

namespace BoothDotDev.Pages.Shared.Partials;

/// <summary>
///     Represents the data shown by the admin media manager: the files of a project or creation, with what is needed to
///     add, remove, reorder, and choose a cover from them.
/// </summary>
public sealed class MediaManager
{
    /// <summary>
    ///     Gets the ID of the project or creation whose files are managed.
    /// </summary>
    /// <value>The owner's ID.</value>
    public required Guid OwnerId { get; init; }

    /// <summary>
    ///     Gets the owner's files, in order.
    /// </summary>
    /// <value>The files.</value>
    public required IReadOnlyList<MediaItem> Items { get; init; }

    /// <summary>
    ///     Gets the local URL of the page to come back to after each change.
    /// </summary>
    /// <value>The return URL.</value>
    public required string ReturnUrl { get; init; }

    /// <summary>
    ///     Gets the problems from the last change, such as files that were rejected.
    /// </summary>
    /// <value>The error text, or <see langword="null" /> if there were none.</value>
    public string? Errors { get; init; }
}
