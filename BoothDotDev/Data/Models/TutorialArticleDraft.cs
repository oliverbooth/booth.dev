namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a single immutable snapshot of a tutorial article's content, taken at the moment it was saved.
/// </summary>
public sealed class TutorialArticleDraft : IEquatable<TutorialArticleDraft>
{
    /// <summary>
    ///     Gets the date and time this draft was saved.
    /// </summary>
    /// <value>The date and time this draft was saved.</value>
    public DateTimeOffset CreatedAt { get; internal set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets the body of the article, as of this draft.
    /// </summary>
    /// <value>The body of the article.</value>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the excerpt of the article, as of this draft, if it has one.
    /// </summary>
    /// <value>The excerpt, or <see langword="null" /> if this draft has no excerpt.</value>
    public string? Excerpt { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the folder the article is contained within, as of this draft.
    /// </summary>
    /// <value>The ID of the folder.</value>
    public Guid Folder { get; set; }

    /// <summary>
    ///     Gets the ID of this draft.
    /// </summary>
    /// <value>The ID of this draft.</value>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets or sets the URL of the article's preview image, as of this draft.
    /// </summary>
    /// <value>The preview image URL.</value>
    public Uri? PreviewImageUrl { get; set; }

    /// <summary>
    ///     Gets or sets the rank of the article within its folder, as of this draft.
    /// </summary>
    /// <value>The rank.</value>
    public int Rank { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to show the table of contents for the article, as of this draft.
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
    ///     Gets or sets the title of the article, as of this draft.
    /// </summary>
    /// <value>The title of the article.</value>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the ID of the article this draft belongs to.
    /// </summary>
    /// <value>The ID of the parent article.</value>
    public Guid TutorialArticleId { get; internal set; }

    /// <summary>
    ///     Gets or sets the visibility of the article, as of this draft.
    /// </summary>
    /// <value>The visibility of the article.</value>
    public Visibility Visibility { get; set; }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="TutorialArticleDraft" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="TutorialArticleDraft" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="TutorialArticleDraft" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(TutorialArticleDraft? left, TutorialArticleDraft? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="TutorialArticleDraft" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="TutorialArticleDraft" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="TutorialArticleDraft" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(TutorialArticleDraft? left, TutorialArticleDraft? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="TutorialArticleDraft" /> is equal to another instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(TutorialArticleDraft? other)
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
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="TutorialArticleDraft" /> and equals
    ///     the value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is TutorialArticleDraft other && Equals(other);
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
