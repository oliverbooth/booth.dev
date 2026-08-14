namespace BoothDotDev.CodeBlockTrivia.Highlighting;

/// <summary>
///     Represents a parsed line specification — a single line, or a range of lines — from a highlight trivia token
///     (e.g. the <c>L3</c> or <c>L3-L5</c> in <c>L3-L5@2..8</c>).
/// </summary>
/// <param name="Start">The first (or only) line bound.</param>
/// <param name="End">
///     The last line bound, or <see langword="null" /> if this is a single-line spec (e.g. <c>L3</c>).
/// </param>
public readonly record struct LineSpec(SpecBound Start, SpecBound? End)
{
    /// <summary>
    ///     Gets a value indicating whether this line spec covers more than one line.
    /// </summary>
    /// <value><see langword="true" /> if this line spec covers a range of lines; otherwise, <see langword="false" />.</value>
    public bool IsRange
    {
        get => End is not null;
    }
}
