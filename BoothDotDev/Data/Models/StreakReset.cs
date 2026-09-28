namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a single day a <see cref="StreakMode.CheckOut" /> streak was broken, restarting its count from
///     that day.
/// </summary>
public sealed class StreakReset
{
    /// <summary>
    ///     Gets the unique identifier for the reset.
    /// </summary>
    /// <value>The unique identifier.</value>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets or sets the ID of the streak this reset belongs to.
    /// </summary>
    /// <value>The streak ID.</value>
    public Guid StreakId { get; set; }

    /// <summary>
    ///     Gets or sets the calendar day the streak was broken.
    /// </summary>
    /// <value>The day.</value>
    public DateOnly OccurredOn { get; set; }

    /// <summary>
    ///     Gets or sets a short note about this reset.
    /// </summary>
    /// <value>The note, or <see langword="null" /> if there isn't one.</value>
    public string? Note { get; set; }
}
