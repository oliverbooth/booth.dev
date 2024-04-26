namespace OliverBooth.Data.Web;

/// <inheritdoc />
internal sealed class CodeSnippet : ICodeSnippet
{
    /// <inheritdoc />
    public string Content { get; } = string.Empty;

    /// <inheritdoc />
    public int Id { get; }

    /// <inheritdoc />
    public string Language { get; } = string.Empty;
}
