namespace BoothDotDev.Data;

/// <summary>
///     Represents the content of a someday entry draft snapshot.
/// </summary>
/// <param name="Title">The title of the entry - the completion of "Someday, ...", without that prefix.</param>
/// <param name="Body">The body of the entry.</param>
/// <param name="Visibility">The visibility of the entry.</param>
public sealed record SomedayEntryDraftContent(string Title, string Body, Visibility Visibility);

/// <summary>
///     Represents a request to create or save a someday entry, bundling its parent-level fields with the content
///     of the draft the save produces.
/// </summary>
/// <param name="Slug">The slug of the entry.</param>
/// <param name="SortOrder">The entry's position on the someday page.</param>
/// <param name="Content">The content of the draft this save produces.</param>
public sealed record SomedayEntrySaveRequest(string Slug, int SortOrder, SomedayEntryDraftContent Content);
