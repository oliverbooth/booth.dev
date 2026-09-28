namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a single immutable snapshot of a streak's content, taken at the moment it was saved.
/// </summary>
public sealed class StreakDraft : IEquatable<StreakDraft>, IMarkdownBody
{
    /// <summary>
    ///     Gets or sets the number of <see cref="CadenceUnit" />s a check-in is expected within, as of this draft.
    /// </summary>
    /// <value>The cadence interval, or <see langword="null" /> if the streak's mode is <see cref="StreakMode.CheckOut" />.</value>
    public int? CadenceInterval { get; set; }

    /// <summary>
    ///     Gets or sets the cadence unit a check-in is expected every <see cref="CadenceInterval" /> of, as of this
    ///     draft.
    /// </summary>
    /// <value>The cadence unit, or <see langword="null" /> if the streak's mode is <see cref="StreakMode.CheckOut" />.</value>
    public StreakCadenceUnit? CadenceUnit { get; set; }

    /// <summary>
    ///     Gets or sets the colour of the streak's card, as of this draft.
    /// </summary>
    /// <value>The explicitly-assigned colour, or <see langword="null" /> to fall back to its position on the page.</value>
    public PaletteHue? Color { get; set; }

    /// <summary>
    ///     Gets the date and time this draft was saved.
    /// </summary>
    /// <value>The date and time this draft was saved.</value>
    public DateTimeOffset CreatedAt { get; internal set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets the colour of the streak's dots, as of this draft.
    /// </summary>
    /// <value>The explicitly-assigned colour, or <see langword="null" /> to fall back to <see cref="Color" />.</value>
    public PaletteHue? DotColor { get; set; }

    /// <summary>
    ///     Gets the ID of this draft.
    /// </summary>
    /// <value>The ID of this draft.</value>
    public Guid Id { get; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets the ID of the streak this draft belongs to.
    /// </summary>
    /// <value>The ID of the parent streak.</value>
    public Guid StreakId { get; internal set; }

    /// <summary>
    ///     Gets or sets the title of the streak, as of this draft.
    /// </summary>
    /// <value>The title of the streak.</value>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the visibility of the streak, as of this draft.
    /// </summary>
    /// <value>The visibility of the streak.</value>
    public Visibility Visibility { get; set; }

    /// <inheritdoc />
    public string Body { get; set; } = string.Empty;

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="StreakDraft" /> is equal to another
    ///     instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(StreakDraft? other)
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
    ///     Returns a value indicating whether two instances of <see cref="StreakDraft" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="StreakDraft" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="StreakDraft" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(StreakDraft? left, StreakDraft? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="StreakDraft" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="StreakDraft" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="StreakDraft" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(StreakDraft? left, StreakDraft? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance is equal to a specified object.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="StreakDraft" /> and
    ///     equals the value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is StreakDraft other && Equals(other));
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
