namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a folder for tutorial articles.
/// </summary>
public sealed class TutorialFolder : IEquatable<TutorialFolder>
{
    /// <summary>
    ///     Gets or sets the description of this folder.
    /// </summary>
    /// <value>The description of this folder.</value>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets the ID of this folder.
    /// </summary>
    /// <value>The ID of the folder.</value>
    public Guid Id { get; private set; }

    /// <summary>
    ///     Gets or sets the ID of this folder's parent.
    /// </summary>
    /// <value>The ID of the parent, or <see langword="null" /> if this folder is at the root.</value>
    public Guid? Parent { get; set; }

    /// <summary>
    ///     Gets or sets the URL of the folder's preview image.
    /// </summary>
    /// <value>The preview image URL.</value>
    public Uri? PreviewImageUrl { get; set; }

    /// <summary>
    ///     Gets or sets the rank of this article within its folder.
    /// </summary>
    /// <value>The rank.</value>
    public int Rank { get; set; }

    /// <summary>
    ///     Gets or sets the slug of this folder.
    /// </summary>
    /// <value>The slug.</value>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the title of this folder.
    /// </summary>
    /// <value>The title.</value>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the visibility of this article.
    /// </summary>
    /// <value>The visibility of the article.</value>
    public Visibility Visibility { get; set; }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="TutorialFolder" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="TutorialFolder" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="TutorialFolder" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(TutorialFolder? left, TutorialFolder? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="TutorialFolder" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="TutorialFolder" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="TutorialFolder" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(TutorialFolder? left, TutorialFolder? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="TutorialFolder" /> is equal to another
    ///     instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(TutorialFolder? other)
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
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="TutorialFolder" /> and
    ///     equals the value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is TutorialFolder other && Equals(other);
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
