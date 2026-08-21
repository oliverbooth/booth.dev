namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a single immutable snapshot of a blog post's content, taken at the moment it was saved.
/// </summary>
/// <remarks>
///     Rows in this table are never updated after they're inserted — every save, whether "Save as draft" or
///     "Save changes", creates a new row. A <see cref="BlogPost" /> points at whichever row is currently live via
///     <see cref="BlogPost.CurrentDraftId" />. Rolling back to an old revision doesn't touch this table either;
///     it loads an old draft's content back into the editor, and the next ordinary save is what persists it as a
///     new row.
/// </remarks>
public sealed class BlogPostDraft : IEquatable<BlogPostDraft>, IMarkdownExcerpt
{
    /// <summary>
    ///     Gets the ID of the blog post this draft belongs to.
    /// </summary>
    /// <value>The ID of the parent blog post.</value>
    public Guid BlogPostId { get; internal set; }

    /// <summary>
    ///     Gets or sets the body of the post, as of this draft.
    /// </summary>
    /// <value>The body of the post.</value>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the category ID of the post, as of this draft.
    /// </summary>
    /// <value>The category ID of the post.</value>
    public Guid CategoryId { get; set; }

    /// <summary>
    ///     Gets the date and time this draft was saved.
    /// </summary>
    /// <value>The date and time this draft was saved.</value>
    public DateTimeOffset CreatedAt { get; internal set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets the excerpt of the post, as of this draft, if it has one.
    /// </summary>
    /// <value>The excerpt, or <see langword="null" /> if this draft has no excerpt.</value>
    public string? Excerpt { get; set; }

    /// <summary>
    ///     Gets the ID of this draft.
    /// </summary>
    /// <value>The ID of this draft.</value>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets or sets a value indicating whether to show the table of contents for the post, as of this draft.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the table of contents should be shown; otherwise, <see langword="false" />.
    /// </value>
    public bool ShowTableOfContents { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the table of contents is expanded by default, as of this draft.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the table of contents is expanded by default; otherwise, <see langword="false" />.
    /// </value>
    public bool TableOfContentsExpanded { get; set; } = true;

    /// <summary>
    ///     Gets or sets the tags of the post, as of this draft.
    /// </summary>
    /// <value>The tags of the post.</value>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    ///     Gets or sets the title of the post, as of this draft.
    /// </summary>
    /// <value>The title of the post.</value>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the visibility of the post, as of this draft.
    /// </summary>
    /// <value>The visibility of the post.</value>
    public Visibility Visibility { get; set; }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="BlogPostDraft" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="BlogPostDraft" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="BlogPostDraft" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(BlogPostDraft? left, BlogPostDraft? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="BlogPostDraft" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="BlogPostDraft" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="BlogPostDraft" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(BlogPostDraft? left, BlogPostDraft? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="BlogPostDraft" /> is equal to another
    ///     instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(BlogPostDraft? other)
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
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="BlogPostDraft" /> and
    ///     equals the value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is BlogPostDraft other && Equals(other);
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
