using BoothDotDev.Common.Data;
using BoothDotDev.Common.Data.Web;

namespace BoothDotDev.Data.Web;

/// <summary>
///     Represents a folder for tutorial articles.
/// </summary>
internal sealed class TutorialFolder : IEquatable<TutorialFolder>, ITutorialFolder
{
    /// <inheritdoc />
    public string? Description { get; private set; }

    /// <inheritdoc />
    public Guid Id { get; private set; }

    /// <inheritdoc />
    public Guid? Parent { get; private set; }

    /// <inheritdoc />
    public Uri? PreviewImageUrl { get; private set; }

    /// <inheritdoc />
    public string Slug { get; private set; } = string.Empty;

    /// <inheritdoc />
    public string Title { get; private set; } = string.Empty;

    /// <inheritdoc />
    public Visibility Visibility { get; private set; }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="TutorialFolder" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="TutorialFolder" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="TutorialFolder" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(TutorialFolder? left, TutorialFolder? right) => Equals(left, right);

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="TutorialFolder" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="TutorialFolder" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="TutorialFolder" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(TutorialFolder? left, TutorialFolder? right) => !(left == right);

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
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
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
