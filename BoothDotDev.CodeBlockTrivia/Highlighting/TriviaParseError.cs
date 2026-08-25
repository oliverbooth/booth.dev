namespace BoothDotDev.CodeBlockTrivia.Highlighting;

/// <summary>
///     Indicates why a highlight trivia string failed to parse.
/// </summary>
public enum TriviaParseError
{
    /// <summary>
    ///     No error; parsing succeeded.
    /// </summary>
    None,

    /// <summary>
    ///     The trivia string was empty or contained only whitespace after the <c>=</c>.
    /// </summary>
    EmptySpec,

    /// <summary>
    ///     The trivia string contained whitespace, which is not permitted within a single block.
    /// </summary>
    UnexpectedWhitespace,

    /// <summary>
    ///     A line or column bound could not be parsed as an integer.
    /// </summary>
    InvalidNumber,

    /// <summary>
    ///     A range's end bound was before its start bound (e.g. <c>L5-L3</c>).
    /// </summary>
    InvertedRange,

    /// <summary>
    ///     The trivia string was malformed in a way not covered by a more specific error.
    /// </summary>
    Malformed
}
