using Markdig.Syntax.Inlines;

namespace BoothDotDev.Extensions.Markdig.Markdown.Template;

/// <summary>
///     Represents a Markdown inline element that represents a MediaWiki-style template.
/// </summary>
public sealed class TemplateInline : Inline
{
    /// <summary>
    ///     Gets the raw argument string.
    /// </summary>
    /// <value>The raw argument string.</value>
    public string ArgumentString { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the argument list.
    /// </summary>
    /// <value>The argument list.</value>
    public IReadOnlyList<string> ArgumentList { get; set; } = ArraySegment<string>.Empty;

    /// <summary>
    ///     Gets the name of the template.
    /// </summary>
    /// <value>The name of the template.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the template parameters.
    /// </summary>
    /// <value>The template parameters.</value>
    public IReadOnlyDictionary<string, string> Params { get; set; } = null!;

    /// <summary>
    ///     Gets the variant of the template.
    /// </summary>
    /// <value>The variant of the template.</value>
    public string Variant { get; set; } = string.Empty;
}
