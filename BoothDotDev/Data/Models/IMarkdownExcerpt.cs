namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents an entity that contains a markdown excerpt.
/// </summary>
public interface IMarkdownExcerpt : IMarkdownBody
{
    /// <summary>
    ///     Gets the excerpt of the markdown content.
    /// </summary>
    /// <value>The excerpt of the markdown content.</value>
    string? Excerpt { get; }
}
