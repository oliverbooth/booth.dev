namespace BoothDotDev.Data;

/// <summary>
///     Represents the content of a blog post draft snapshot.
/// </summary>
/// <param name="Title">The title of the post.</param>
/// <param name="Body">The body of the post.</param>
/// <param name="Excerpt">The excerpt of the post, if any.</param>
/// <param name="CategoryId">The ID of the post's category.</param>
/// <param name="Visibility">The visibility of the post.</param>
/// <param name="Tags">The tags associated with the post.</param>
/// <param name="ShowTableOfContents">A value indicating whether to show the table of contents for the post.</param>
/// <param name="TableOfContentsExpanded">A value indicating whether the table of contents is expanded by default.</param>
public sealed record BlogPostDraftContent(
    string Title,
    string Body,
    string? Excerpt,
    Guid CategoryId,
    Visibility Visibility,
    IReadOnlyList<string> Tags,
    bool ShowTableOfContents,
    bool TableOfContentsExpanded);

/// <summary>
///     Represents a request to create or save a blog post, bundling its parent-level fields with the content of the
///     draft the save produces.
/// </summary>
/// <param name="AuthorId">The ID of the post's author.</param>
/// <param name="Slug">The URL slug of the post.</param>
/// <param name="PublishedAt">The publication date and time of the post.</param>
/// <param name="EnableComments">A value indicating whether comments are enabled for the post.</param>
/// <param name="Content">The content of the draft this save produces.</param>
public sealed record BlogPostSaveRequest(
    Guid AuthorId,
    string Slug,
    DateTimeOffset PublishedAt,
    bool EnableComments,
    BlogPostDraftContent Content);
