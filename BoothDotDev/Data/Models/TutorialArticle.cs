using System.ComponentModel.DataAnnotations.Schema;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a tutorial article.
/// </summary>
public sealed class TutorialArticle : IEquatable<TutorialArticle>, IMarkdownExcerpt
{
    /// <summary>
    ///     Gets the body of this article, as of its current draft.
    /// </summary>
    /// <value>The body.</value>
    [NotMapped]
    public string Body
    {
        get => Draft.Body;
    }

    /// <summary>
    ///     Gets the draft that is currently live for this article.
    /// </summary>
    /// <value>The currently-live draft.</value>
    public TutorialArticleDraft? CurrentDraft { get; internal set; }

    /// <summary>
    ///     Gets the ID of the draft that is currently live for this article.
    /// </summary>
    /// <value>The ID of the currently-live draft.</value>
    public Guid? CurrentDraftId { get; internal set; }

    /// <summary>
    ///     Gets or sets a value indicating whether comments are enabled for the article.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if comments are enabled for the article; otherwise, <see langword="false" />.
    /// </value>
    public bool EnableComments { get; set; }

    /// <summary>
    ///     Gets the excerpt of this article, as of its current draft, if it has one.
    /// </summary>
    /// <value>The excerpt, or <see langword="null" /> if this article has no excerpt.</value>
    [NotMapped]
    public string? Excerpt
    {
        get => Draft.Excerpt;
    }

    /// <summary>
    ///     Gets the ID of the folder this article is contained within, as of its current draft.
    /// </summary>
    /// <value>The ID of the folder.</value>
    [NotMapped]
    public Guid Folder
    {
        get => Draft.Folder;
    }

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
    ///     Gets the URL of the article's preview image, as of its current draft.
    /// </summary>
    /// <value>The preview image URL.</value>
    [NotMapped]
    public Uri? PreviewImageUrl
    {
        get => Draft.PreviewImageUrl;
    }

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
    ///     Gets the rank of this article within its folder, as of its current draft.
    /// </summary>
    /// <value>The rank.</value>
    [NotMapped]
    public int Rank
    {
        get => Draft.Rank;
    }

    /// <summary>
    ///     Gets or sets the ID of the post that was redirected to this article.
    /// </summary>
    /// <value>The source redirect post ID.</value>
    public Guid? RedirectFrom { get; set; }

    /// <summary>
    ///     Gets a value indicating whether to show the table of contents for the article, as of its current draft.
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
    ///     Gets or sets the slug of this article.
    /// </summary>
    /// <value>The slug.</value>
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
    ///     Gets the title of this article, as of its current draft.
    /// </summary>
    /// <value>The title.</value>
    [NotMapped]
    public string Title
    {
        get => Draft.Title;
    }

    /// <summary>
    ///     Gets or sets the date and time the article was moved to the trash.
    /// </summary>
    /// <value>
    ///     The date and time the article was trashed, or <see langword="null" /> if the article is not trashed.
    /// </value>
    public DateTimeOffset? TrashedAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time at which this article was updated.
    /// </summary>
    /// <value>The update timestamp, or <see langword="null" /> if this article has not been updated.</value>
    public DateTimeOffset? Updated { get; set; }

    /// <summary>
    ///     Gets the visibility of this article, as of its current draft.
    /// </summary>
    /// <value>The visibility of the article.</value>
    [NotMapped]
    public Visibility Visibility
    {
        get => Draft.Visibility;
    }

    /// <summary>
    ///     Gets the currently-live draft, throwing if it has not been loaded.
    /// </summary>
    /// <value>The currently-live draft.</value>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="CurrentDraft" /> was not eager-loaded by the query that produced this instance.
    /// </exception>
    private TutorialArticleDraft Draft
    {
        get => CurrentDraft ?? throw new InvalidOperationException(
            $"The current draft for article '{Id}' was not loaded. Ensure the query includes '{nameof(CurrentDraft)}'.");
    }

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
