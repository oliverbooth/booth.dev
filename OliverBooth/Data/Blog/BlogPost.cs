namespace OliverBooth.Data.Blog;

/// <summary>
///     Represents a blog post.
/// </summary>
public sealed class BlogPost : IEquatable<BlogPost>
{
    /// <summary>
    ///     Gets the ID of the author.
    /// </summary>
    /// <value>The author ID.</value>
    public int AuthorId { get; private set; }

    /// <summary>
    ///     Gets or sets the body of the blog post.
    /// </summary>
    /// <value>The body.</value>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the ID of the blog post.
    /// </summary>
    /// <value>The ID.</value>
    public int Id { get; private set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the blog post is a redirect.
    /// </summary>
    /// <value><see langword="true" /> if the blog post is a redirect; otherwise, <see langword="false" />.</value>
    public bool IsRedirect { get; set; }

    /// <summary>
    ///     Gets or sets the date and time at which the blog post was published.
    /// </summary>
    /// <value>The publish timestamp.</value>
    public DateTimeOffset Published { get; set; }

    /// <summary>
    ///     Gets or sets the redirect URL of the blog post.
    /// </summary>
    /// <value>The redirect URL.</value>
    public string? RedirectUrl { get; set; }

    /// <summary>
    ///     Gets or sets the URL slug of the blog post.
    /// </summary>
    /// <value>The URL slug.</value>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the title of the blog post.
    /// </summary>
    /// <value>The title.</value>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the date and time at which the blog post was updated.
    /// </summary>
    /// <value>The update timestamp.</value>
    public DateTimeOffset? Updated { get; set; }

    /// <summary>
    ///     Gets or sets the legacy WordPress ID of the blog post.
    /// </summary>
    /// <value>The legacy WordPress ID.</value>
    public int? WordPressId { get; set; }

    public bool Equals(BlogPost? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is BlogPost other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id;
    }
}
