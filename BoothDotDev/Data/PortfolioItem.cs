namespace BoothDotDev.Data;

/// <summary>
///     Represents the kind of work a <see cref="PortfolioItem" /> stands for.
/// </summary>
public enum PortfolioItemKind
{
    /// <summary>
    ///     A code project.
    /// </summary>
    Project,

    /// <summary>
    ///     A piece of artwork.
    /// </summary>
    Artwork,

    /// <summary>
    ///     A piece of music.
    /// </summary>
    Music
}

/// <summary>
///     Represents one card of things I've made, whatever kind of work it is, so projects and creations can be listed
///     and ordered together.
/// </summary>
public sealed record PortfolioItem
{
    /// <summary>
    ///     Gets or initializes the kind of work the item stands for.
    /// </summary>
    /// <value>The kind of work.</value>
    public required PortfolioItemKind Kind { get; init; }

    /// <summary>
    ///     Gets or initializes the ID of the creation the item stands for.
    /// </summary>
    /// <value>The creation ID, or <see langword="null" /> if the item is a project.</value>
    public Guid? Id { get; init; }

    /// <summary>
    ///     Gets or initializes the date the creation was published.
    /// </summary>
    /// <value>The publication date, or <see langword="null" /> if the item is a project.</value>
    public DateTimeOffset? PublishedAt { get; init; }

    /// <summary>
    ///     Gets or initializes the title of the item.
    /// </summary>
    /// <value>The title.</value>
    public required string Title { get; init; }

    /// <summary>
    ///     Gets or initializes a plain-text description of the item.
    /// </summary>
    /// <value>The description, or <see langword="null" /> if it has none.</value>
    public string? Description { get; init; }

    /// <summary>
    ///     Gets or initializes the description of the item as Markdown, for showing it in full.
    /// </summary>
    /// <value>The Markdown description, or <see langword="null" /> if the item has none.</value>
    public string? DescriptionMarkdown { get; init; }

    /// <summary>
    ///     Gets or initializes the URL of the item's image.
    /// </summary>
    /// <value>The image URL, or <see langword="null" /> if the item has no image.</value>
    public string? ImageUrl { get; init; }

    /// <summary>
    ///     Gets or initializes the URL of the item's audio file.
    /// </summary>
    /// <value>The audio URL, or <see langword="null" /> if the item is not music.</value>
    public string? AudioUrl { get; init; }

    /// <summary>
    ///     Gets or initializes the length of the item's audio.
    /// </summary>
    /// <value>The duration, or <see langword="null" /> if the item is not music.</value>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    ///     Gets or initializes the path of the page the item links to.
    /// </summary>
    /// <value>The Razor page path.</value>
    public required string PagePath { get; init; }

    /// <summary>
    ///     Gets or initializes the route values for the item's page.
    /// </summary>
    /// <value>The route values.</value>
    public Dictionary<string, string> RouteValues { get; init; } = [];

    /// <summary>
    ///     Gets or initializes the colour the item is shown in.
    /// </summary>
    /// <value>The colour.</value>
    public required PaletteHue Hue { get; init; }

    /// <summary>
    ///     Gets or initializes the text of the pill shown on the item.
    /// </summary>
    /// <value>The name of the kind of work, such as <c>code</c> or <c>music</c>.</value>
    public required string Label { get; init; }

    /// <summary>
    ///     Gets or initializes small labels shown along the bottom of the item, such as languages or tools.
    /// </summary>
    /// <value>The tags.</value>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    ///     Gets or initializes the status of the project.
    /// </summary>
    /// <value>The project status, or <see langword="null" /> if the item is not a project.</value>
    public ProjectStatus? Status { get; init; }

    /// <summary>
    ///     Gets or initializes a value indicating whether the item is a work in progress.
    /// </summary>
    /// <value><see langword="true" /> if the item is a work in progress; otherwise, <see langword="false" />.</value>
    public bool IsWorkInProgress { get; init; }

    /// <summary>
    ///     Gets or initializes the heights, in pixels, of the bars of the decorative waveform shown on music items.
    /// </summary>
    /// <value>The bar heights, or an empty list if the item is not music.</value>
    public IReadOnlyList<int> WaveformBars { get; init; } = [];
}
