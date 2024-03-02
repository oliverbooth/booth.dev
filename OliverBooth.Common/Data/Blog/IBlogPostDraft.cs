namespace OliverBooth.Common.Data.Blog;

/// <summary>
///     Represents a draft of a blog post.
/// </summary>
public interface IBlogPostDraft
{
    /// <summary>
    ///     Gets the author of the post.
    /// </summary>
    /// <value>The author of the post.</value>
    IAuthor Author { get; }

    /// <summary>
    ///     Gets or sets the body of the post.
    /// </summary>
    /// <value>The body of the post.</value>
    string Body { get; set; }

    /// <summary>
    ///     Gets a value indicating whether comments are enabled for the post.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if comments are enabled for the post; otherwise, <see langword="false" />.
    /// </value>
    bool EnableComments { get; }

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
    ///     Gets the URL to which the post redirects.
    /// </summary>
    /// <value>The URL to which the post redirects, or <see langword="null" /> if the post does not redirect.</value>
    Uri? RedirectUrl { get; }

    /// <summary>
    ///     Gets the slug of the post.
    /// </summary>
    /// <value>The slug of the post.</value>
    string Slug { get; }

    /// <summary>
    ///     Gets the tags of the post.
    /// </summary>
    /// <value>The tags of the post.</value>
    IReadOnlyList<string> Tags { get; }

    /// <summary>
    ///     Gets or sets the title of the post.
    /// </summary>
    /// <value>The title of the post.</value>
    string Title { get; set; }

    /// <summary>
    ///     Gets the date and time the post was last updated.
    /// </summary>
    /// <value>The update date and time.</value>
    DateTimeOffset Updated { get; }

    /// <summary>
    ///     Gets the visibility of the post.
    /// </summary>
    /// <value>The visibility of the post.</value>
    BlogPostVisibility Visibility { get; }

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
    ///     Gets the Disqus post ID for the post.
    /// </summary>
    /// <returns>The Disqus post ID for the post.</returns>
    string GetDisqusPostId();
}
