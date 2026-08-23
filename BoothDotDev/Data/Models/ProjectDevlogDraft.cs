namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a single immutable snapshot of a devlog entry's content, taken at the moment it was saved.
/// </summary>
public sealed class ProjectDevlogDraft : IEquatable<ProjectDevlogDraft>
{
    /// <summary>
    ///     Gets or sets the body of the devlog entry, as of this draft.
    /// </summary>
    /// <value>The body.</value>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the date and time this draft was saved.
    /// </summary>
    /// <value>The date and time this draft was saved.</value>
    public DateTimeOffset CreatedAt { get; internal set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets the ID of this draft.
    /// </summary>
    /// <value>The ID of this draft.</value>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets the ID of the devlog entry this draft belongs to.
    /// </summary>
    /// <value>The ID of the parent devlog entry.</value>
    public Guid ProjectDevlogId { get; internal set; }

    /// <summary>
    ///     Gets or sets the title of the devlog entry, as of this draft.
    /// </summary>
    /// <value>The title.</value>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the visibility of the devlog entry, as of this draft.
    /// </summary>
    /// <value>The visibility.</value>
    public Visibility Visibility { get; set; }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="ProjectDevlogDraft" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="ProjectDevlogDraft" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="ProjectDevlogDraft" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(ProjectDevlogDraft? left, ProjectDevlogDraft? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="ProjectDevlogDraft" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="ProjectDevlogDraft" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="ProjectDevlogDraft" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(ProjectDevlogDraft? left, ProjectDevlogDraft? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="ProjectDevlogDraft" /> is equal to another
    ///     instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(ProjectDevlogDraft? other)
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
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="ProjectDevlogDraft" /> and
    ///     equals the value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is ProjectDevlogDraft other && Equals(other);
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
