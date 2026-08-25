namespace BoothDotDev.Data;

/// <summary>
///     Represents the content of a devlog entry draft snapshot.
/// </summary>
/// <param name="Title">The title of the devlog entry.</param>
/// <param name="Body">The body of the devlog entry.</param>
/// <param name="Visibility">The visibility of the devlog entry.</param>
public sealed record ProjectDevlogDraftContent(string Title, string Body, Visibility Visibility);

/// <summary>
///     Represents a request to create or save a devlog entry, bundling its parent-level fields with the content of
///     the draft the save produces.
/// </summary>
/// <param name="ProjectId">The ID of the project this devlog entry belongs to.</param>
/// <param name="Slug">The slug of the devlog entry.</param>
/// <param name="PublishedAt">The publication date and time of the devlog entry.</param>
/// <param name="EnableComments">A value indicating whether comments are enabled for the devlog entry.</param>
/// <param name="Content">The content of the draft this save produces.</param>
public sealed record ProjectDevlogSaveRequest(
    Guid ProjectId,
    string Slug,
    DateTimeOffset PublishedAt,
    bool EnableComments,
    ProjectDevlogDraftContent Content);
