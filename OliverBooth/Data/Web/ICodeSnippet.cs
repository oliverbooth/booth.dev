namespace OliverBooth.Data.Web;

/// <summary>
///     Represents a code snippet.
/// </summary>
public interface ICodeSnippet
{
    /// <summary>
    ///     Gets the content for this snippet.
    /// </summary>
    /// <value>The content for this snippet</value>
    string Content { get; }

    /// <summary>
    ///     Gets the ID for this snippet.
    /// </summary>
    /// <value>The ID for this snippet</value>
    int Id { get; }

    /// <summary>
    ///     Gets the language for this snippet.
    /// </summary>
    /// <value>The language for this snippet</value>
    string Language { get; }
}
