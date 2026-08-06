using System.ComponentModel.DataAnnotations.Schema;
using BoothDotDev.Common.Data;
using BoothDotDev.Common.Data.Models;
using SmartFormat;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a blog post.
/// </summary>
public sealed class BlogPost
{
    /// <summary>
    ///     Gets the author of the post.
    /// </summary>
    /// <value>The author of the post.</value>
    [NotMapped]
    public User Author { get; internal set; } = null!;

    /// <summary>
    ///     Gets or sets the body of the post.
    /// </summary>
    /// <value>The body of the post.</value>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether comments are enabled for the post.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if comments are enabled for the post; otherwise, <see langword="false" />.
    /// </value>
    public bool EnableComments { get; set; }

    /// <summary>
    ///     Gets or sets the excerpt of this post, if it has one.
    /// </summary>
    /// <value>The excerpt, or <see langword="null" /> if this post has no excerpt.</value>
    public string? Excerpt { get; set; }

    /// <summary>
    ///     Gets the ID of the post.
    /// </summary>
    /// <value>The ID of the post.</value>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    ///     Gets or sets a value indicating whether the post redirects to another URL.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the post redirects to another URL; otherwise, <see langword="false" />.
    /// </value>
    public bool IsRedirect { get; set; }

    /// <summary>
    ///     Gets or sets the password of the post.
    /// </summary>
    /// <value>The password of the post.</value>
    public string? Password { get; set; }

    /// <summary>
    ///     Gets the date and time the post was published.
    /// </summary>
    /// <value>The publication date and time.</value>
    public DateTimeOffset Published { get; internal set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets the URL to which the post redirects.
    /// </summary>
    /// <value>The URL to which the post redirects, or <see langword="null" /> if the post does not redirect.</value>
    public Uri? RedirectUrl { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to show the table of contents for the post.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the table of contents should be shown; otherwise, <see langword="false" />.
    /// </value>
    public bool ShowTableOfContents { get; set; }

    /// <summary>
    ///     Gets or sets the slug of the post.
    /// </summary>
    /// <value>The slug of the post.</value>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether the table of contents is expanded by default.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the table of contents is expanded by default; otherwise, <see langword="false" />.
    /// </value>
    public bool TableOfContentsExpanded { get; set; } = true;

    /// <summary>
    ///     Gets or sets the tags of the post.
    /// </summary>
    /// <value>The tags of the post.</value>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    ///     Gets or sets the title of the post.
    /// </summary>
    /// <value>The title of the post.</value>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the type of the post.
    /// </summary>
    /// <value>The type of the post.</value>
    public BlogPostType Type { get; set; }

    /// <summary>
    ///     Gets or sets the date and time the post was last updated.
    /// </summary>
    /// <value>The update date and time, or <see langword="null" /> if the post has not been updated.</value>
    public DateTimeOffset? Updated { get; set; }

    /// <summary>
    ///     Gets or sets the visibility of the post.
    /// </summary>
    /// <value>The visibility of the post.</value>
    public Visibility Visibility { get; set; }

    /// <summary>
    ///     Gets the WordPress ID of the post.
    /// </summary>
    /// <value>
    ///     The WordPress ID of the post, or <see langword="null" /> if the post was not imported from WordPress.
    /// </value>
    public int? WordPressId { get; internal set; }

    /// <summary>
    ///     Gets or sets the ID of the author of this blog post.
    /// </summary>
    /// <value>The ID of the author of this blog post.</value>
    internal Guid AuthorId { get; set; }

    /// <summary>
    ///     Gets or sets the base URL of the Disqus comments for the blog post.
    /// </summary>
    /// <value>The Disqus base URL.</value>
    internal string? DisqusDomain { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the Disqus comments for the blog post.
    /// </summary>
    /// <value>The Disqus identifier.</value>
    internal string? DisqusIdentifier { get; set; }

    /// <summary>
    ///     Gets or sets the URL path of the Disqus comments for the blog post.
    /// </summary>
    /// <value>The Disqus URL path.</value>
    internal string? DisqusPath { get; set; }

    /// <summary>
    ///     Gets the Disqus domain for the blog post.
    /// </summary>
    /// <returns>The Disqus domain.</returns>
    public string GetDisqusDomain()
    {
        return string.IsNullOrWhiteSpace(DisqusDomain)
            ? "https://booth.dev/blog"
            : Smart.Format(DisqusDomain, this);
    }

    /// <summary>
    ///     Gets the Disqus identifier for the post.
    /// </summary>
    /// <returns>The Disqus identifier for the post.</returns>
    public string GetDisqusIdentifier()
    {
        return string.IsNullOrWhiteSpace(DisqusIdentifier) ? $"post-{Id}" : Smart.Format(DisqusIdentifier, this);
    }

    /// <summary>
    ///     Gets the Disqus URL for the post.
    /// </summary>
    /// <returns>The Disqus URL for the post.</returns>
    public string GetDisqusUrl()
    {
        string path = string.IsNullOrWhiteSpace(DisqusPath)
            ? $"{Published:yyyy/MM/dd}/{Slug}/"
            : Smart.Format(DisqusPath, this);

        return $"{GetDisqusDomain()}/{path}";
    }

    /// <summary>
    ///     Gets the Disqus post ID for the post.
    /// </summary>
    /// <returns>The Disqus post ID for the post.</returns>
    public string GetDisqusPostId()
    {
        return WordPressId?.ToString() ?? Id.ToString();
    }
}
