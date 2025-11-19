namespace BoothDotDev.Common.Data.Web;

/// <summary>
///     Represents a tutorial article.
/// </summary>
public interface ITutorialArticle
{
    /// <summary>
    ///     Gets the body of this article.
    /// </summary>
    /// <value>The body.</value>
    string Body { get; }

    /// <summary>
    ///     Gets a value indicating whether comments are enabled for the article.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if comments are enabled for the article; otherwise, <see langword="false" />.
    /// </value>
    bool EnableComments { get; }

    /// <summary>
    ///     Gets the excerpt of this article, if it has one.
    /// </summary>
    /// <value>The excerpt, or <see langword="null" /> if this article has no excerpt.</value>
    string? Excerpt { get; }

    /// <summary>
    ///     Gets the ID of the folder this article is contained within.
    /// </summary>
    /// <value>The ID of the folder.</value>
    Guid Folder { get; }

    /// <summary>
    ///     Gets a value indicating whether this article is part of a multi-part series.
    /// </summary>
    /// <value><see langword="true" /> if this article has additional parts; otherwise, <see langword="false" />.</value>
    bool HasOtherParts { get; }

    /// <summary>
    ///     Gets the ID of this article.
    /// </summary>
    /// <value>The ID.</value>
    Guid Id { get; }

    /// <summary>
    ///     Gets the ID of the next article to this one.
    /// </summary>
    /// <value>The next part ID.</value>
    Guid? NextPart { get; }

    /// <summary>
    ///     Gets the URL of the article's preview image.
    /// </summary>
    /// <value>The preview image URL.</value>
    Uri? PreviewImageUrl { get; }

    /// <summary>
    ///     Gets the ID of the previous article to this one.
    /// </summary>
    /// <value>The previous part ID.</value>
    Guid? PreviousPart { get; }

    /// <summary>
    ///     Gets the date and time at which this article was published.
    /// </summary>
    /// <value>The publish timestamp.</value>
    DateTimeOffset Published { get; }

    /// <summary>
    ///     Gets the ID of the post that was redirected to this article.
    /// </summary>
    /// <value>The source redirect post ID.</value>
    Guid? RedirectFrom { get; }

    /// <summary>
    ///     Gets the slug of this article.
    /// </summary>
    /// <value>The slug.</value>
    string Slug { get; }

    /// <summary>
    ///     Gets the title of this article.
    /// </summary>
    /// <value>The title.</value>
    string Title { get; }

    /// <summary>
    ///     Gets the date and time at which this article was updated.
    /// </summary>
    /// <value>The update timestamp, or <see langword="null" /> if this article has not been updated.</value>
    DateTimeOffset? Updated { get; }

    /// <summary>
    ///     Gets the visibility of this article.
    /// </summary>
    /// <value>The visibility of the article.</value>
    Visibility Visibility { get; }
}
