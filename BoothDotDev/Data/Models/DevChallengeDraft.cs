using DEDrake;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a single immutable snapshot of a challenge's content, taken at the moment it was saved.
/// </summary>
public sealed class DevChallengeDraft : IEquatable<DevChallengeDraft>
{
    /// <summary>
    ///     Gets the date and time this draft was saved.
    /// </summary>
    /// <value>The date and time this draft was saved.</value>
    public DateTimeOffset CreatedAt { get; internal set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets the description of the challenge, as of this draft.
    /// </summary>
    /// <value>The description of the challenge.</value>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the ID of the challenge this draft belongs to.
    /// </summary>
    /// <value>The ID of the parent challenge.</value>
    public ShortGuid DevChallengeId { get; internal set; }

    /// <summary>
    ///     Gets or sets the excerpt of the challenge, as of this draft.
    /// </summary>
    /// <value>
    ///     The excerpt of the challenge, or <see langword="null" /> if none was set - a preview is then auto-derived
    ///     from <see cref="Description" /> instead.
    /// </value>
    public string? Excerpt { get; set; }

    /// <summary>
    ///     Gets the ID of this draft.
    /// </summary>
    /// <value>The ID of this draft.</value>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets or sets a value indicating whether the solution should be shown, as of this draft.
    /// </summary>
    /// <value><see langword="true" /> if the solution should be shown; otherwise, <see langword="false" />.</value>
    public bool ShowSolution { get; set; }

    /// <summary>
    ///     Gets or sets the solution for the challenge, as of this draft.
    /// </summary>
    /// <value>The solution for the challenge.</value>
    public string? Solution { get; set; }

    /// <summary>
    ///     Gets or sets the title of the challenge, as of this draft.
    /// </summary>
    /// <value>The title of the challenge.</value>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the visibility of the challenge, as of this draft.
    /// </summary>
    /// <value>The visibility of the challenge.</value>
    public Visibility Visibility { get; set; }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="DevChallengeDraft" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="DevChallengeDraft" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="DevChallengeDraft" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(DevChallengeDraft? left, DevChallengeDraft? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="DevChallengeDraft" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="DevChallengeDraft" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="DevChallengeDraft" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(DevChallengeDraft? left, DevChallengeDraft? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="DevChallengeDraft" /> is equal to another
    ///     instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(DevChallengeDraft? other)
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
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="DevChallengeDraft" /> and
    ///     equals the value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is DevChallengeDraft other && Equals(other);
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
