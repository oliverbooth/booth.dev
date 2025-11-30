namespace BoothDotDev.Common.Data.Blog;

/// <summary>
///     Represents a blog post.
/// </summary>
public interface IBlogPost
{
    /// <summary>
    ///     Gets the author of the post.
    /// </summary>
    /// <value>The author of the post.</value>
    IBlogAuthor Author { get; }

    /// <summary>
    ///     Gets the body of the post.
    /// </summary>
    /// <value>The body of the post.</value>
    string Body { get; }

    /// <summary>
    ///     Gets a value indicating whether comments are enabled for the post.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if comments are enabled for the post; otherwise, <see langword="false" />.
    /// </value>
    bool EnableComments { get; }

    /// <summary>
    ///     Gets the excerpt of this post, if it has one.
    /// </summary>
    /// <value>The excerpt, or <see langword="null" /> if this post has no excerpt.</value>
    string? Excerpt { get; }

    /// <summary>
    ///     Gets the ID of the post.
    /// </summary>
    /// <value>The ID of the post.</value>
    Guid Id { get; }

    /// <summary>
    ///     Gets a value indicating whether the post redirects to another URL.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the post redirects to another URL; otherwise, <see langword="false" />.
    /// </value>
    bool IsRedirect { get; }

    /// <summary>
    ///     Gets the password of the post.
    /// </summary>
    /// <value>The password of the post.</value>
    string? Password { get; }

    /// <summary>
    ///     Gets the date and time the post was published.
    /// </summary>
    /// <value>The publication date and time.</value>
    DateTimeOffset Published { get; }

    /// <summary>
    ///     Gets the URL to which the post redirects.
    /// </summary>
    /// <value>The URL to which the post redirects, or <see langword="null" /> if the post does not redirect.</value>
    Uri? RedirectUrl { get; }

    /// <summary>
    ///     Gets a value indicating whether to show the table of contents for the post.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the table of contents should be shown; otherwise, <see langword="false" />.
    /// </value>
    bool ShowTableOfContents { get; }

    /// <summary>
    ///     Gets the slug of the post.
    /// </summary>
    /// <value>The slug of the post.</value>
    string Slug { get; }

    /// <summary>
    ///     Gets a value indicating whether the table of contents is expanded by default.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the table of contents is expanded by default; otherwise, <see langword="false" />.
    /// </value>
    bool TableOfContentsExpanded { get; }

    /// <summary>
    ///     Gets the tags of the post.
    /// </summary>
    /// <value>The tags of the post.</value>
    IReadOnlyList<string> Tags { get; }

    /// <summary>
    ///     Gets the title of the post.
    /// </summary>
    /// <value>The title of the post.</value>
    string Title { get; }

    /// <summary>
    ///     Gets the date and time the post was last updated.
    /// </summary>
    /// <value>The update date and time, or <see langword="null" /> if the post has not been updated.</value>
    DateTimeOffset? Updated { get; }

    /// <summary>
    ///     Gets the visibility of the post.
    /// </summary>
    /// <value>The visibility of the post.</value>
    Visibility Visibility { get; }

    /// <summary>
    ///     Gets the WordPress ID of the post.
    /// </summary>
    /// <value>
    ///     The WordPress ID of the post, or <see langword="null" /> if the post was not imported from WordPress.
    /// </value>
    int? WordPressId { get; }

    /// <summary>
    ///     Gets the Disqus identifier for the post.
    /// </summary>
    /// <returns>The Disqus identifier for the post.</returns>
    string GetDisqusIdentifier();

    /// <summary>
    ///     Gets the Disqus URL for the post.
    /// </summary>
    /// <returns>The Disqus URL for the post.</returns>
    string GetDisqusUrl();

    /// <summary>
    ///     Gets the Disqus post ID for the post.
    /// </summary>
    /// <returns>The Disqus post ID for the post.</returns>
    string GetDisqusPostId();
}
