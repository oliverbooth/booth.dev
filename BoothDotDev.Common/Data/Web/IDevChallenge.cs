using DEDrake;

namespace BoothDotDev.Common.Data.Web;

/// <summary>
///     Represents a development challenge.
/// </summary>
public interface IDevChallenge
{
    /// <summary>
    ///     Gets the date of the challenge.
    /// </summary>
    /// <value>The date of the challenge.</value>
    DateTimeOffset Date { get; }

    /// <summary>
    ///     Gets the description of the challenge.
    /// </summary>
    /// <value>The description of the challenge.</value>
    string Description { get; }

    /// <summary>
    ///     Gets the ID of the challenge.
    /// </summary>
    /// <value>The ID of the challenge.</value>
    ShortGuid Id { get; }

    /// <summary>
    ///     Gets the old ID of the challenge.
    /// </summary>
    /// <value>The old ID of the challenge.</value>
    int? OldId { get; }

    /// <summary>
    ///     Gets the password of the challenge.
    /// </summary>
    /// <value>The password of the challenge.</value>
    string? Password { get; }

    /// <summary>
    ///     Gets a value indicating whether to show the solution to the challenge.
    /// </summary>
    /// <value><see langword="true" /> if the solution should be shown; otherwise, <see langword="false" />.</value>
    bool ShowSolution { get; }

    /// <summary>
    ///     Gets the solution to the challenge.
    /// </summary>
    /// <value>The solution to the challenge.</value>
    string? Solution { get; }

    /// <summary>
    ///     Gets the title of the challenge.
    /// </summary>
    /// <value>The title of the challenge.</value>
    string Title { get; }

    /// <summary>
    ///     Gets the visibility of the challenge.
    /// </summary>
    /// <value>The visibility of the challenge.</value>
    Visibility Visibility { get; }
}
