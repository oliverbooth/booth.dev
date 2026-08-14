namespace BoothDotDev.CodeBlockTrivia.Highlighting;

/// <summary>
///     Represents an inclusive range of 1-indexed bounds, as parsed from a highlight trivia spec
///     (e.g. <c>2..8</c> or <c>2..^2</c>).
/// </summary>
/// <param name="Start">The start bound.</param>
/// <param name="End">The end bound.</param>
public readonly record struct SpecRange(SpecBound Start, SpecBound End)
{
    /// <summary>
    ///     Converts this <see cref="SpecRange" /> to a <see cref="System.Range" />, suitable for slicing a 0-indexed,
    ///     exclusive-end <see cref="ReadOnlySpan{T}" /> of <see langword="char" />.
    /// </summary>
    /// <returns>The equivalent <see cref="System.Range" />.</returns>
    public Range ToRange()
    {
        return new Range(Start.ToStartIndex(), End.ToEndIndex());
    }
}
