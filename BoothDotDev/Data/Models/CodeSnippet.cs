namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a code snippet.
/// </summary>
public sealed class CodeSnippet
{
    /// <summary>
    ///     Gets or sets the content for this snippet.
    /// </summary>
    /// <value>The content for this snippet</value>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the ID for this snippet.
    /// </summary>
    /// <value>The ID for this snippet</value>
    public int Id { get; }

    /// <summary>
    ///     Gets or sets the language for this snippet.
    /// </summary>
    /// <value>The language for this snippet</value>
    public string Language { get; set; } = string.Empty;
}
