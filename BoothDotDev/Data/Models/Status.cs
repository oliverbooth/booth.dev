namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a status that can be shown in the thought bubble on the homepage.
/// </summary>
public sealed class Status
{
    /// <summary>
    ///     The maximum length of <see cref="Text" />.
    /// </summary>
    public const int MaxTextLength = 200;

    /// <summary>
    ///     Gets the unique identifier for the status.
    /// </summary>
    /// <value>The unique identifier.</value>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets or sets the text of the status.
    /// </summary>
    /// <value>The text, as plain text.</value>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether the status is in rotation, and so can be shown on the homepage.
    /// </summary>
    /// <value><see langword="true" /> if the status can be shown; otherwise, <see langword="false" />.</value>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Gets the date and time the status was added.
    /// </summary>
    /// <value>The date and time the status was added.</value>
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
