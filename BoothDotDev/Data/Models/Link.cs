namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a link from a project or a creation to somewhere else.
/// </summary>
/// <remarks>A link belongs to exactly one of <see cref="Project" /> or <see cref="Creation" />.</remarks>
public sealed class Link
{
    /// <summary>
    ///     Gets the unique identifier for the link.
    /// </summary>
    /// <value>The unique identifier.</value>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets or sets the ID of the project the link belongs to.
    /// </summary>
    /// <value>The project ID, or <see langword="null" /> if the link belongs to a creation.</value>
    public Guid? ProjectId { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the creation the link belongs to.
    /// </summary>
    /// <value>The creation ID, or <see langword="null" /> if the link belongs to a project.</value>
    public Guid? CreationId { get; set; }

    /// <summary>
    ///     Gets or sets the kind of place the link leads to.
    /// </summary>
    /// <value>The kind.</value>
    public LinkKind Kind { get; set; }

    /// <summary>
    ///     Gets or sets the address the link leads to.
    /// </summary>
    /// <value>The URL, which is always an absolute <c>http</c> or <c>https</c> address.</value>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the text shown on the link in place of the default for its kind.
    /// </summary>
    /// <value>The label, or <see langword="null" /> to use the default.</value>
    public string? Label { get; set; }

    /// <summary>
    ///     Gets or sets the position of the link among its owner's links, counting from zero.
    /// </summary>
    /// <value>The position.</value>
    public int Position { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the link is its owner's primary link, shown first and given the most
    ///     prominence.
    /// </summary>
    /// <value><see langword="true" /> if the link is the primary one; otherwise, <see langword="false" />.</value>
    /// <remarks>An owner has at most one primary link, and no owner has to have one.</remarks>
    public bool IsPrimary { get; set; }
}
