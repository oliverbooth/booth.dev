namespace BoothDotDev.Pages.Shared.Partials;

/// <summary>
///     Represents the data shown in the header of a content page: the badge and tags, the title, and the byline.
/// </summary>
public sealed class ContentHeader
{
    /// <summary>
    ///     Gets the title of the content.
    /// </summary>
    /// <value>The title.</value>
    public required string Title { get; init; }

    /// <summary>
    ///     Gets the text of the badge shown before the tags, such as the category or content type.
    /// </summary>
    /// <value>The badge text, or <see langword="null" /> to show no badge.</value>
    public string? Badge { get; init; }

    /// <summary>
    ///     Gets the CSS class modifier selecting the badge's colour, such as <c>badge-magenta</c>.
    /// </summary>
    /// <value>The badge class. The default is an empty string, which is the default teal badge.</value>
    public string BadgeClass { get; init; } = string.Empty;

    /// <summary>
    ///     Gets the <c>data-hue</c> attribute value driving the badge's solid fill and the title's ink colour.
    /// </summary>
    /// <value>The hue's <c>data-hue</c> value, or <see langword="null" /> to render with no hue at all.</value>
    public string? Hue { get; init; }

    /// <summary>
    ///     Gets the text of a chip shown right after the badge, tinted with the header's hue, such as a challenge's
    ///     difficulty.
    /// </summary>
    /// <value>The chip text, or <see langword="null" /> to show no chip.</value>
    public string? Rating { get; init; }

    /// <summary>
    ///     Gets the tags to link to the blog's tag filter.
    /// </summary>
    /// <value>The tags.</value>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    ///     Gets the display name of the author.
    /// </summary>
    /// <value>The author's display name, or <see langword="null" /> to show no author.</value>
    public string? AuthorName { get; init; }

    /// <summary>
    ///     Gets the URL of the author's avatar.
    /// </summary>
    /// <value>The avatar URL, or <see langword="null" /> to show only the author's initial.</value>
    public Uri? AuthorAvatarUrl { get; init; }

    /// <summary>
    ///     Gets the pieces of metadata shown under the author's name, joined with a separator.
    /// </summary>
    /// <value>The metadata, such as the publish date and reading time.</value>
    public IReadOnlyList<string> Meta { get; init; } = [];

    /// <summary>
    ///     Gets a highlighted piece of metadata shown after <see cref="Meta" />, such as a state worth drawing the eye to.
    /// </summary>
    /// <value>The highlighted metadata, or <see langword="null" /> to show none.</value>
    public MetaFlag? Flag { get; init; }

    /// <summary>
    ///     Gets the URL of the content's Markdown source.
    /// </summary>
    /// <value>The source URL, or <see langword="null" /> to show no link.</value>
    public string? SourceUrl { get; init; }

    /// <summary>
    ///     Gets the URL of the admin edit page for the content.
    /// </summary>
    /// <value>The edit URL, or <see langword="null" /> to show no link (for visitors who can't edit).</value>
    public string? EditUrl { get; init; }

    /// <summary>
    ///     Gets a value indicating whether to show the reader a toggle to switch this content's voice font
    ///     (<c>prose--serif</c>) to the default sans-serif for readability.
    /// </summary>
    /// <value><see langword="true" /> to show the toggle; otherwise, <see langword="false" />.</value>
    public bool ShowVoiceFontToggle { get; init; }
}

/// <summary>
///     Represents a piece of header metadata shown in a hue with an icon, in place of plain text.
/// </summary>
/// <param name="Text">The text to show.</param>
/// <param name="Icon">The Tabler icon class shown before the text, such as <c>ti-check</c>.</param>
/// <param name="Hue">The <c>data-hue</c> value the text and icon take their colour from.</param>
public sealed record MetaFlag(string Text, string Icon, string Hue);
