namespace BoothDotDev.Data;

/// <summary>
///     Represents one item of the portfolio as an RSS feed needs it.
/// </summary>
public sealed record PortfolioFeedItem
{
    /// <summary>
    ///     Gets or initializes the unique identifier of the project or creation.
    /// </summary>
    /// <value>The unique identifier.</value>
    public required Guid Id { get; init; }

    /// <summary>
    ///     Gets or initializes a value indicating whether the item is a project rather than a creation.
    /// </summary>
    /// <value><see langword="true" /> if the item is a project; otherwise, <see langword="false" />.</value>
    public required bool IsProject { get; init; }

    /// <summary>
    ///     Gets or initializes the slug of the item's page.
    /// </summary>
    /// <value>The slug.</value>
    public required string Slug { get; init; }

    /// <summary>
    ///     Gets or initializes the title of the item.
    /// </summary>
    /// <value>The title.</value>
    public required string Title { get; init; }

    /// <summary>
    ///     Gets or initializes the description of the item.
    /// </summary>
    /// <value>The description, as Markdown, or <see langword="null" /> if the item has none.</value>
    public string? Description { get; init; }

    /// <summary>
    ///     Gets or initializes the date the item was made or first published.
    /// </summary>
    /// <value>The date.</value>
    public required DateTimeOffset Date { get; init; }
}
