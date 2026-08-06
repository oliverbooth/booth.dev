using BoothDotDev.Common.Data.Models;

namespace BoothDotDev.Data.Models;

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
