namespace BoothDotDev.Data;

/// <summary>
///     Represents the content of a tutorial article draft snapshot.
/// </summary>
/// <param name="Title">The title of the article.</param>
/// <param name="Body">The body of the article.</param>
/// <param name="Excerpt">The excerpt of the article, if it has one.</param>
/// <param name="Folder">The ID of the folder the article is contained within.</param>
/// <param name="Rank">The rank of the article within its folder.</param>
/// <param name="PreviewImageUrl">The URL of the article's preview image.</param>
/// <param name="ShowTableOfContents">A value indicating whether the table of contents should be shown.</param>
/// <param name="TableOfContentsExpanded">A value indicating whether the table of contents is expanded by default.</param>
/// <param name="Visibility">The visibility of the article.</param>
public sealed record TutorialArticleDraftContent(
    string Title,
    string Body,
    string? Excerpt,
    Guid Folder,
    int Rank,
    Uri? PreviewImageUrl,
    bool ShowTableOfContents,
    bool TableOfContentsExpanded,
    Visibility Visibility);

/// <summary>
///     Represents a request to create or save a tutorial article, bundling its parent-level fields with the content
///     of the draft the save produces.
/// </summary>
/// <param name="Slug">The slug of the article.</param>
/// <param name="PublishedAt">The publication date and time of the article.</param>
/// <param name="EnableComments">A value indicating whether comments are enabled for the article.</param>
/// <param name="NextPart">The ID of the next article to this one, if this article is part of a series.</param>
/// <param name="PreviousPart">The ID of the previous article to this one, if this article is part of a series.</param>
/// <param name="RedirectFrom">The ID of the post that was redirected to this article, if any.</param>
/// <param name="Content">The content of the draft this save produces.</param>
public sealed record TutorialArticleSaveRequest(
    string Slug,
    DateTimeOffset PublishedAt,
    bool EnableComments,
    Guid? NextPart,
    Guid? PreviousPart,
    Guid? RedirectFrom,
    TutorialArticleDraftContent Content);
