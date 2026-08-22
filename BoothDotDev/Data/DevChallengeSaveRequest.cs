namespace BoothDotDev.Data;

/// <summary>
///     Represents the content of a challenge draft snapshot.
/// </summary>
/// <param name="Title">The title of the challenge.</param>
/// <param name="Description">The description of the challenge.</param>
/// <param name="Solution">The solution for the challenge.</param>
/// <param name="ShowSolution">A value indicating whether the solution should be shown.</param>
/// <param name="Visibility">The visibility of the challenge.</param>
public sealed record DevChallengeDraftContent(
    string Title,
    string Description,
    string? Solution,
    bool ShowSolution,
    Visibility Visibility);

/// <summary>
///     Represents a request to create or save a challenge, bundling its parent-level fields with the content of the
///     draft the save produces.
/// </summary>
/// <param name="PublishedAt">The publication date and time of the challenge.</param>
/// <param name="NewPassword">
///     The new password for the challenge, or <see langword="null" /> or whitespace to leave the existing password
///     (if any) unchanged.
/// </param>
/// <param name="Content">The content of the draft this save produces.</param>
public sealed record DevChallengeSaveRequest(DateTimeOffset PublishedAt, string? NewPassword, DevChallengeDraftContent Content);
