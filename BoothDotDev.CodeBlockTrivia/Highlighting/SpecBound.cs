namespace BoothDotDev.CodeBlockTrivia.Highlighting;

/// <summary>
///     Represents a single 1-indexed bound, either from the start or from the end, as parsed from a highlight trivia spec
///     (e.g. the <c>2</c> in <c>2..8</c>, or the <c>2</c> in <c>^2</c>).
/// </summary>
/// <param name="Value">The 1-indexed value of the bound.</param>
/// <param name="IsFromEnd">
///     <see langword="true" /> if this bound counts from the end (i.e. was written with a <c>^</c> prefix); otherwise,
///     <see langword="false" />.
/// </param>
public readonly record struct SpecBound(int Value, bool IsFromEnd)
{
    /// <summary>
    ///     Creates a <see cref="SpecBound" /> counting from the start (forward), e.g. spec <c>2</c>.
    /// </summary>
    /// <param name="value">The 1-indexed value.</param>
    /// <returns>A new <see cref="SpecBound" /> instance.</returns>
    public static SpecBound Forward(int value)
    {
        return new SpecBound(value, IsFromEnd: false);
    }

    /// <summary>
    ///     Creates a <see cref="SpecBound" /> counting from the end (backward), e.g. spec <c>^2</c>.
    /// </summary>
    /// <param name="value">The 1-indexed value, counted from the end.</param>
    /// <returns>A new <see cref="SpecBound" /> instance.</returns>
    public static SpecBound Backward(int value)
    {
        return new SpecBound(value, IsFromEnd: true);
    }

    /// <summary>
    ///     Converts this bound to a <see cref="System.Index" />, treating it as the <b>start</b> of a range.
    /// </summary>
    /// <returns>The equivalent <see cref="System.Index" />.</returns>
    public Index ToStartIndex()
    {
        return IsFromEnd
            ? new Index(Value, fromEnd: true) // spec back-index is already aligned with Index's fromEnd semantics
            : new Index(Value - 1); // spec is 1-based, Index is 0-based
    }

    /// <summary>
    ///     Converts this bound to a <see cref="System.Index" />, treating it as the <b>end</b> of a range.
    /// </summary>
    /// <returns>The equivalent <see cref="System.Index" />.</returns>
    public Index ToEndIndex()
    {
        return IsFromEnd
            ? new Index(Value - 1, fromEnd: true) // spec end is inclusive, Range.End is exclusive
            : new Index(Value); // 1-based inclusive-end coincides with 0-based exclusive-end
    }
}
