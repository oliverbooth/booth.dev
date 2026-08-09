using DEDrake;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a development challenge.
/// </summary>
public sealed class DevChallenge
{
    /// <summary>
    ///     Gets the date of the challenge.
    /// </summary>
    /// <value>The date of the challenge.</value>
    public DateTimeOffset Date { get; private set; }

    /// <summary>
    ///     Gets or sets the description of the challenge.
    /// </summary>
    /// <value>The description of the challenge.</value>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the ID of the challenge.
    /// </summary>
    /// <value>The ID of the challenge.</value>
    public ShortGuid Id { get; private set; }

    /// <summary>
    ///     Gets the old ID of the challenge.
    /// </summary>
    /// <value>The old ID of the challenge.</value>
    public int? OldId { get; private set; }

    /// <summary>
    ///     Gets or sets the password for the challenge.
    /// </summary>
    /// <value>The password for the challenge.</value>
    public string? Password { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the solution should be shown.
    /// </summary>
    /// <value><see langword="true" /> if the solution should be shown; otherwise, <see langword="false" />.</value>
    public bool ShowSolution { get; set; }

    /// <summary>
    ///     Gets or sets the solution for the challenge.
    /// </summary>
    /// <value>The solution for the challenge.</value>
    public string? Solution { get; set; }

    /// <summary>
    ///     Gets or sets the title of the challenge.
    /// </summary>
    /// <value>The title of the challenge.</value>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the visibility of the challenge.
    /// </summary>
    /// <value>The visibility of the challenge.</value>
    public Visibility Visibility { get; set; }
}
