namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents an entity that contains a markdown body.
/// </summary>
public interface IMarkdownBody
{
    /// <summary>
    ///     Gets the body of the markdown content.
    /// </summary>
    /// <value>The body of the markdown content.</value>
    string Body { get; }
}
