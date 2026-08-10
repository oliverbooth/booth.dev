using System.ComponentModel.DataAnnotations.Schema;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a blog post.
/// </summary>
public sealed class BlogPost : IEquatable<BlogPost>, IMarkdownExcerpt
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
    ///     Gets or sets the category ID of the post.
    /// </summary>
    /// <value>The category ID of the post.</value>
    public Guid CategoryId { get; set; }

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
    public Guid Id { get; private set; } = Guid.CreateVersion7();

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
    ///     Returns a value indicating whether two instances of <see cref="BlogPost" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="BlogPost" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="BlogPost" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(BlogPost? left, BlogPost? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="BlogPost" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="BlogPost" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="BlogPost" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(BlogPost? left, BlogPost? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="BlogPost" /> is equal to another
    ///     instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(BlogPost? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Id.Equals(other.Id);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance is equal to a specified object.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="BlogPost" /> and
    ///     equals the value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is BlogPost other && Equals(other);
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
