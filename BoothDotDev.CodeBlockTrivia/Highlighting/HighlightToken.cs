namespace BoothDotDev.CodeBlockTrivia.Highlighting;

/// <summary>
///     Represents a single parsed highlight token — a line spec, optionally paired with one or more column ranges
///     (e.g. <c>L1@(2..8,14..20)</c>).
/// </summary>
/// <param name="Lines">The line spans this token applies to.</param>
/// <param name="IsGrouped">
///     <see langword="true" /> if the line span(s) were wrapped in parentheses, meaning any attached column spec applies
///     uniformly across every line in every span, rather than only the last line of the last span.
/// </param>
/// <param name="Columns">
///     The column ranges to highlight, or <see langword="null" /> if the entire line(s) should be highlighted with no column
///     restriction.
/// </param>
public readonly record struct HighlightToken(IReadOnlyList<LineSpec> Lines, bool IsGrouped, IReadOnlyList<SpecRange>? Columns);
