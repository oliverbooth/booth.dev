namespace BoothDotDev.Data;

/// <summary>
///     Represents the content of a note draft snapshot.
/// </summary>
/// <param name="Title">The title of the note.</param>
/// <param name="Content">The content of the note.</param>
/// <param name="FontStyle">The font style of the note.</param>
/// <param name="Visibility">The visibility of the note.</param>
public sealed record NoteDraftContent(string Title, string Content, FontStyle FontStyle, Visibility Visibility);

/// <summary>
///     Represents a request to create or save a note, bundling its parent-level fields with the content of the
///     draft the save produces.
/// </summary>
/// <param name="PublishedAt">The publication date and time of the note.</param>
/// <param name="Content">The content of the draft this save produces.</param>
public sealed record NoteSaveRequest(DateTimeOffset PublishedAt, NoteDraftContent Content);
