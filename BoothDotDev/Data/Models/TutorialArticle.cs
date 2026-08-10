using System.ComponentModel.DataAnnotations.Schema;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a tutorial article.
/// </summary>
public sealed class TutorialArticle : IEquatable<TutorialArticle>, IMarkdownExcerpt
{
    /// <summary>
    ///     Gets or sets the body of this article.
    /// </summary>
    /// <value>The body.</value>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether comments are enabled for the article.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if comments are enabled for the article; otherwise, <see langword="false" />.
    /// </value>
    public bool EnableComments { get; set; }

    /// <summary>
    ///     Gets or sets the excerpt of this article, if it has one.
    /// </summary>
    /// <value>The excerpt, or <see langword="null" /> if this article has no excerpt.</value>
    public string? Excerpt { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the folder this article is contained within.
    /// </summary>
    /// <value>The ID of the folder.</value>
    public Guid Folder { get; set; }

    /// <summary>
    ///     Gets a value indicating whether this article is part of a multi-part series.
    /// </summary>
    /// <value><see langword="true" /> if this article has additional parts; otherwise, <see langword="false" />.</value>
    [NotMapped]
    public bool HasOtherParts
    {
        get => NextPart is not null || PreviousPart is not null;
    }

    /// <summary>
    ///     Gets the ID of this article.
    /// </summary>
    /// <value>The ID.</value>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets or sets the ID of the next article to this one.
    /// </summary>
    /// <value>The next part ID.</value>
    public Guid? NextPart { get; set; }

    /// <summary>
    ///     Gets or sets the URL of the article's preview image.
    /// </summary>
    /// <value>The preview image URL.</value>
    public Uri? PreviewImageUrl { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the previous article to this one.
    /// </summary>
    /// <value>The previous part ID.</value>
    public Guid? PreviousPart { get; set; }

    /// <summary>
    ///     Gets the date and time at which this article was published.
    /// </summary>
    /// <value>The publish timestamp.</value>
    public DateTimeOffset Published { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets the rank of this article within its folder.
    /// </summary>
    /// <value>The rank.</value>
    public int Rank { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the post that was redirected to this article.
    /// </summary>
    /// <value>The source redirect post ID.</value>
    public Guid? RedirectFrom { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to show the table of contents for the post.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the table of contents should be shown; otherwise, <see langword="false" />.
    /// </value>
    public bool ShowTableOfContents { get; set; }

    /// <summary>
    ///     Gets or sets the slug of this article.
    /// </summary>
    /// <value>The slug.</value>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether the table of contents is expanded by default.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the table of contents is expanded by default; otherwise, <see langword="false" />.
    /// </value>
    public bool TableOfContentsExpanded { get; set; } = true;

    /// <summary>
    ///     Gets or sets the title of this article.
    /// </summary>
    /// <value>The title.</value>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the date and time at which this article was updated.
    /// </summary>
    /// <value>The update timestamp, or <see langword="null" /> if this article has not been updated.</value>
    public DateTimeOffset? Updated { get; set; }

    /// <summary>
    ///     Gets or sets the visibility of this article.
    /// </summary>
    /// <value>The visibility of the article.</value>
    public Visibility Visibility { get; set; }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="TutorialArticle" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="TutorialArticle" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="TutorialArticle" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(TutorialArticle? left, TutorialArticle? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="TutorialArticle" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="TutorialArticle" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="TutorialArticle" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(TutorialArticle? left, TutorialArticle? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="TutorialArticle" /> is equal to another
    ///     instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(TutorialArticle? other)
    {
        if (ReferenceEquals(null, other))
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
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="TutorialArticle" /> and
    ///     equals the value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is TutorialArticle other && Equals(other);
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
