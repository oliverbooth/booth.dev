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
    ///     Gets the body of the post, as of its current draft.
    /// </summary>
    /// <value>The body of the post.</value>
    [NotMapped]
    public string Body
    {
        get => Draft.Body;
    }

    /// <summary>
    ///     Gets the category ID of the post, as of its current draft.
    /// </summary>
    /// <value>The category ID of the post.</value>
    [NotMapped]
    public Guid CategoryId
    {
        get => Draft.CategoryId;
    }

    /// <summary>
    ///     Gets the draft that is currently live for this post.
    /// </summary>
    /// <value>The currently-live draft.</value>
    public BlogPostDraft? CurrentDraft { get; internal set; }

    /// <summary>
    ///     Gets the ID of the draft that is currently live for this post.
    /// </summary>
    /// <value>The ID of the currently-live draft.</value>
    public Guid? CurrentDraftId { get; internal set; }

    /// <summary>
    ///     Gets or sets a value indicating whether comments are enabled for the post.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if comments are enabled for the post; otherwise, <see langword="false" />.
    /// </value>
    public bool EnableComments { get; set; }

    /// <summary>
    ///     Gets the excerpt of this post, as of its current draft, if it has one.
    /// </summary>
    /// <value>The excerpt, or <see langword="null" /> if this post has no excerpt.</value>
    [NotMapped]
    public string? Excerpt
    {
        get => Draft.Excerpt;
    }

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
    ///     Gets the date and time the post was published.
    /// </summary>
    /// <value>The publication date and time.</value>
    public DateTimeOffset PublishedAt { get; internal set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets the URL to which the post redirects.
    /// </summary>
    /// <value>The URL to which the post redirects, or <see langword="null" /> if the post does not redirect.</value>
    public Uri? RedirectUrl { get; set; }

    /// <summary>
    ///     Gets a value indicating whether to show the table of contents for the post, as of its current draft.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the table of contents should be shown; otherwise, <see langword="false" />.
    /// </value>
    [NotMapped]
    public bool ShowTableOfContents
    {
        get => Draft.ShowTableOfContents;
    }

    /// <summary>
    ///     Gets or sets the slug of the post.
    /// </summary>
    /// <value>The slug of the post.</value>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    ///     Gets a value indicating whether the table of contents is expanded by default, as of its current draft.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the table of contents is expanded by default; otherwise, <see langword="false" />.
    /// </value>
    [NotMapped]
    public bool TableOfContentsExpanded
    {
        get => Draft.TableOfContentsExpanded;
    }

    /// <summary>
    ///     Gets the tags of the post, as of its current draft.
    /// </summary>
    /// <value>The tags of the post.</value>
    [NotMapped]
    public List<string> Tags
    {
        get => Draft.Tags;
    }

    /// <summary>
    ///     Gets the title of the post, as of its current draft.
    /// </summary>
    /// <value>The title of the post.</value>
    [NotMapped]
    public string Title
    {
        get => Draft.Title;
    }

    /// <summary>
    ///     Gets or sets the date and time the post was moved to the trash.
    /// </summary>
    /// <value>The date and time the post was trashed, or <see langword="null" /> if the post is not trashed.</value>
    /// <remarks>
    ///     A trashed post is hidden from every listing and 404s on its public URL regardless of
    ///     <see cref="Visibility" />, but is otherwise untouched and can be restored. It is not the same as
    ///     <see cref="Visibility.Private" />, which the author can still browse to directly.
    /// </remarks>
    public DateTimeOffset? TrashedAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time the post was last updated, i.e. the last time <see cref="CurrentDraftId" />
    ///     changed.
    /// </summary>
    /// <value>The update date and time, or <see langword="null" /> if the post has not been updated.</value>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    ///     Gets the visibility of the post, as of its current draft.
    /// </summary>
    /// <value>The visibility of the post.</value>
    [NotMapped]
    public Visibility Visibility
    {
        get => Draft.Visibility;
    }

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
    ///     Gets the currently-live draft, throwing if it has not been loaded.
    /// </summary>
    /// <value>The currently-live draft.</value>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="CurrentDraft" /> was not eager-loaded by the query that produced this instance.
    /// </exception>
    private BlogPostDraft Draft
    {
        get => CurrentDraft ?? throw new InvalidOperationException(
            $"The current draft for blog post '{Id}' was not loaded. Ensure the query includes '{nameof(CurrentDraft)}'.");
    }

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
