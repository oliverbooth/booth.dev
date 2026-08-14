namespace BoothDotDev.CodeBlockTrivia.Highlighting;

/// <summary>
///     Represents a single trivia block parsed from a code fence's info string (e.g. the <c>h=...</c> portion).
/// </summary>
/// <param name="Kind">
///     The trivia kind prefix (e.g. <c>"h"</c> for highlight). Reserved for future trivia kinds beyond highlighting.
/// </param>
/// <param name="Tokens">The comma-separated tokens parsed from this block.</param>
public readonly record struct TriviaBlock(string Kind, IReadOnlyList<HighlightToken> Tokens);
