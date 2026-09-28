using System.ComponentModel.DataAnnotations.Schema;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents something being tracked for consecutive days, weeks, or another cadence (e.g. a Duolingo streak).
/// </summary>
public sealed class Streak : IEquatable<Streak>, IMarkdownBody
{
    /// <summary>
    ///     Gets the cadence unit a check-in is expected every <see cref="CadenceInterval" /> of, as of its current
    ///     draft.
    /// </summary>
    /// <value>The cadence unit, or <see langword="null" /> if <see cref="Mode" /> is <see cref="StreakMode.CheckOut" />.</value>
    [NotMapped]
    public StreakCadenceUnit? CadenceUnit
    {
        get => Draft.CadenceUnit;
    }

    /// <summary>
    ///     Gets the number of <see cref="CadenceUnit" />s a check-in is expected within, as of its current draft.
    /// </summary>
    /// <value>The cadence interval, or <see langword="null" /> if <see cref="Mode" /> is <see cref="StreakMode.CheckOut" />.</value>
    [NotMapped]
    public int? CadenceInterval
    {
        get => Draft.CadenceInterval;
    }

    /// <summary>
    ///     Gets the colour of the streak's card, as of its current draft.
    /// </summary>
    /// <value>The explicitly-assigned colour, or <see langword="null" /> to fall back to its position on the page.</value>
    [NotMapped]
    public PaletteHue? Color
    {
        get => Draft.Color;
    }

    /// <summary>
    ///     Gets the draft that is currently live for this streak.
    /// </summary>
    /// <value>The currently-live draft.</value>
    public StreakDraft? CurrentDraft { get; internal set; }

    /// <summary>
    ///     Gets the ID of the draft that is currently live for this streak.
    /// </summary>
    /// <value>The ID of the currently-live draft.</value>
    public Guid? CurrentDraftId { get; internal set; }

    /// <summary>
    ///     Gets the colour of the streak's dots, as of its current draft.
    /// </summary>
    /// <value>The explicitly-assigned colour, or <see langword="null" /> to fall back to <see cref="Color" />.</value>
    [NotMapped]
    public PaletteHue? DotColor
    {
        get => Draft.DotColor;
    }

    /// <summary>
    ///     Gets the ID of the streak.
    /// </summary>
    /// <value>The ID of the streak.</value>
    public Guid Id { get; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets or sets how this streak's current and best counts are derived.
    /// </summary>
    /// <value>The streak's tracking mode.</value>
    /// <remarks>
    ///     A parent-level field, like <see cref="StartedOn" />: it governs how the streak's logged history is
    ///     interpreted, not its wording, so changing it never produces a draft.
    /// </remarks>
    public StreakMode Mode { get; set; }

    /// <summary>
    ///     Gets the date and time the streak was first published.
    /// </summary>
    /// <value>The publication date and time.</value>
    public DateTimeOffset PublishedAt { get; internal set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets the slug of the streak, used as its anchor ID on the streaks page and in its own URL.
    /// </summary>
    /// <value>The slug of the streak.</value>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the streak's position on the streaks page, relative to every other streak.
    /// </summary>
    /// <value>The sort order of the streak. Lower values are rendered first.</value>
    public int SortOrder { get; set; }

    /// <summary>
    ///     Gets or sets the date this streak started - e.g. the date a habit was quit.
    /// </summary>
    /// <value>The start date, or <see langword="null" /> if <see cref="Mode" /> is <see cref="StreakMode.CheckIn" />.</value>
    /// <remarks>
    ///     Only meaningful for a <see cref="StreakMode.CheckOut" /> streak: it's the anchor its current count counts
    ///     up from until the first <see cref="StreakReset" /> is logged. A <see cref="StreakMode.CheckIn" /> streak
    ///     has no equivalent - its count is anchored by its check-in log instead.
    /// </remarks>
    public DateOnly? StartedOn { get; set; }

    /// <summary>
    ///     Gets the title of the streak, as of its current draft.
    /// </summary>
    /// <value>The title of the streak.</value>
    [NotMapped]
    public string Title
    {
        get => Draft.Title;
    }

    /// <summary>
    ///     Gets or sets the date and time the streak was moved to the trash.
    /// </summary>
    /// <value>The date and time the streak was trashed, or <see langword="null" /> if the streak is not trashed.</value>
    public DateTimeOffset? TrashedAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time the streak was last updated, i.e. the last time <see cref="CurrentDraftId" />
    ///     changed.
    /// </summary>
    /// <value>The update date and time, or <see langword="null" /> if the streak has not been updated.</value>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    ///     Gets the visibility of the streak, as of its current draft.
    /// </summary>
    /// <value>The visibility of the streak.</value>
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
    private StreakDraft Draft
    {
        get => CurrentDraft ?? throw new InvalidOperationException(
            $"The current draft for streak '{Id}' was not loaded. Ensure the query includes '{nameof(CurrentDraft)}'.");
    }

    /// <inheritdoc />
    [NotMapped]
    public string Body
    {
        get => Draft.Body;
    }

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="Streak" /> is equal to another instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(Streak? other)
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
    ///     Returns a value indicating whether two instances of <see cref="Streak" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="Streak" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="Streak" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(Streak? left, Streak? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="Streak" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="Streak" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="Streak" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(Streak? left, Streak? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance is equal to a specified object.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="Streak" /> and equals the
    ///     value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is Streak other && Equals(other));
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
