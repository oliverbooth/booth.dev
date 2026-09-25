using BoothDotDev.Data;

namespace BoothDotDev.Pages.Shared.Partials;

/// <summary>
///     Represents the data shown by the admin link manager: the links of a project or creation, with what is needed to
///     add, change, remove, and reorder them.
/// </summary>
public sealed class LinkManager
{
    /// <summary>
    ///     Gets the ID of the project or creation whose links are managed.
    /// </summary>
    /// <value>The owner's ID.</value>
    public required Guid OwnerId { get; init; }

    /// <summary>
    ///     Gets the owner's links, in order.
    /// </summary>
    /// <value>The links.</value>
    public required IReadOnlyList<LinkItem> Items { get; init; }

    /// <summary>
    ///     Gets the local URL of the page to come back to after each change.
    /// </summary>
    /// <value>The return URL.</value>
    public required string ReturnUrl { get; init; }

    /// <summary>
    ///     Gets the problems from the last change, such as an address that was rejected.
    /// </summary>
    /// <value>The error text, or <see langword="null" /> if there were none.</value>
    public string? Errors { get; init; }
}
