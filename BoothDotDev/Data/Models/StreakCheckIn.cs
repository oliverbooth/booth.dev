namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a single logged day of a <see cref="StreakMode.CheckIn" /> streak.
/// </summary>
/// <seealso cref="StreakCheckInKind" />
public sealed class StreakCheckIn
{
    /// <summary>
    ///     Gets the unique identifier for the check-in.
    /// </summary>
    /// <value>The unique identifier.</value>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets or sets the ID of the streak this check-in belongs to.
    /// </summary>
    /// <value>The streak ID.</value>
    public Guid StreakId { get; set; }

    /// <summary>
    ///     Gets or sets the calendar day this check-in was for.
    /// </summary>
    /// <value>The day.</value>
    public DateOnly OccurredOn { get; set; }

    /// <summary>
    ///     Gets or sets whether this day counts toward the streak's displayed number.
    /// </summary>
    /// <value>The kind of check-in.</value>
    public StreakCheckInKind Kind { get; set; }

    /// <summary>
    ///     Gets or sets a short note about this check-in.
    /// </summary>
    /// <value>The note, or <see langword="null" /> if there isn't one.</value>
    public string? Note { get; set; }
}
