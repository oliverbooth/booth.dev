using SmartFormat;

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
    public Guid AuthorId { get; private set; }

    /// <summary>
    ///     Gets or sets the body of the blog post.
    /// </summary>
    /// <value>The body.</value>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether comments are enabled for the blog post.
    /// </summary>
    /// <value><see langword="true" /> if comments are enabled; otherwise, <see langword="false" />.</value>
    public bool EnableComments { get; set; } = true;

    /// <summary>
    ///     Gets or sets the base URL of the Disqus comments for the blog post.
    /// </summary>
    /// <value>The Disqus base URL.</value>
    public string? DisqusDomain { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the Disqus comments for the blog post.
    /// </summary>
    /// <value>The Disqus identifier.</value>
    public string? DisqusIdentifier { get; set; }

    /// <summary>
    ///     Gets or sets the URL path of the Disqus comments for the blog post.
    /// </summary>
    /// <value>The Disqus URL path.</value>
    public string? DisqusPath { get; set; }

    /// <summary>
    ///     Gets the ID of the blog post.
    /// </summary>
    /// <value>The ID.</value>
    public Guid Id { get; private set; }

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

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="BlogPost" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="BlogPost" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="BlogPost" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(BlogPost? left, BlogPost? right) => Equals(left, right);

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="BlogPost" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="BlogPost" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="BlogPost" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(BlogPost? left, BlogPost? right) => !(left == right);

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="BlogPost" /> is equal to another instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(BlogPost? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    /// <summary>
    ///     Returns a value indicating whether this instance is equal to a specified object.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="BlogPost" /> and equals the
    ///     value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is BlogPost other && Equals(other);
    }

    /// <summary>
    ///     Gets the Disqus identifier for the blog post.
    /// </summary>
    /// <returns>The Disqus identifier.</returns>
    public string GetDisqusIdentifier()
    {
        return string.IsNullOrWhiteSpace(DisqusIdentifier) ? $"post-{Id}" : Smart.Format(DisqusIdentifier, this);
    }

    /// <summary>
    ///     Gets the Disqus domain for the blog post.
    /// </summary>
    /// <returns>The Disqus domain.</returns>
    public string GetDisqusDomain()
    {
        return string.IsNullOrWhiteSpace(DisqusDomain)
            ? "https://oliverbooth.dev/blog"
            : Smart.Format(DisqusDomain, this);
    }

    /// <summary>
    ///     Gets the Disqus URL for the blog post.
    /// </summary>
    /// <returns>The Disqus URL.</returns>
    public string GetDisqusUrl()
    {
        string path = string.IsNullOrWhiteSpace(DisqusPath)
            ? $"{Published:yyyy/MM/dd}/{Slug}/"
            : Smart.Format(DisqusPath, this);

        return $"{GetDisqusDomain()}/{path}";
    }

    /// <summary>
    ///     Gets the hash code for this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Id.GetHashCode();
    }
}
