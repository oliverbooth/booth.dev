using System.ComponentModel.DataAnnotations.Schema;
using DEDrake;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a development challenge.
/// </summary>
public sealed class DevChallenge : IEquatable<DevChallenge>
{
    /// <summary>
    ///     Gets the draft that is currently live for this challenge.
    /// </summary>
    /// <value>The currently-live draft.</value>
    public DevChallengeDraft? CurrentDraft { get; internal set; }

    /// <summary>
    ///     Gets the ID of the draft that is currently live for this challenge.
    /// </summary>
    /// <value>The ID of the currently-live draft.</value>
    public Guid? CurrentDraftId { get; internal set; }

    /// <summary>
    ///     Gets the description of the challenge, as of its current draft.
    /// </summary>
    /// <value>The description of the challenge.</value>
    [NotMapped]
    public string Description
    {
        get => Draft.Description;
    }

    /// <summary>
    ///     Gets the ID of the challenge.
    /// </summary>
    /// <value>The ID of the challenge.</value>
    public ShortGuid Id { get; private set; } = ShortGuid.NewGuid();

    /// <summary>
    ///     Gets the old ID of the challenge.
    /// </summary>
    /// <value>The old ID of the challenge.</value>
    public int? OldId { get; private set; }

    /// <summary>
    ///     Gets the date and time when the challenge was published.
    /// </summary>
    /// <value>A <see cref="DateTimeOffset" /> representing the date and time when the challenge was published.</value>
    public DateTimeOffset PublishedAt { get; internal set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets a value indicating whether the solution should be shown, as of its current draft.
    /// </summary>
    /// <value><see langword="true" /> if the solution should be shown; otherwise, <see langword="false" />.</value>
    [NotMapped]
    public bool ShowSolution
    {
        get => Draft.ShowSolution;
    }

    /// <summary>
    ///     Gets the solution for the challenge, as of its current draft.
    /// </summary>
    /// <value>The solution for the challenge.</value>
    [NotMapped]
    public string? Solution
    {
        get => Draft.Solution;
    }

    /// <summary>
    ///     Gets the title of the challenge, as of its current draft.
    /// </summary>
    /// <value>The title of the challenge.</value>
    [NotMapped]
    public string Title
    {
        get => Draft.Title;
    }

    /// <summary>
    ///     Gets or sets the date and time the challenge was trashed.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTimeOffset" /> representing when the challenge was trashed, or <see langword="null" /> if it
    ///     isn't trashed.
    /// </value>
    public DateTimeOffset? TrashedAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the challenge was last updated.
    /// </summary>
    /// <value>A <see cref="DateTimeOffset" /> representing the date and time when the challenge was last updated.</value>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    ///     Gets the visibility of the challenge, as of its current draft.
    /// </summary>
    /// <value>The visibility of the challenge.</value>
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
    private DevChallengeDraft Draft
    {
        get => CurrentDraft ?? throw new InvalidOperationException(
            $"The current draft for challenge '{Id}' was not loaded. Ensure the query includes '{nameof(CurrentDraft)}'.");
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="DevChallenge" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="DevChallenge" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="DevChallenge" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(DevChallenge? left, DevChallenge? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="DevChallenge" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="DevChallenge" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="DevChallenge" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(DevChallenge? left, DevChallenge? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="DevChallenge" /> is equal to another instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(DevChallenge? other)
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
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="DevChallenge" /> and equals the
    ///     value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is DevChallenge other && Equals(other);
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
