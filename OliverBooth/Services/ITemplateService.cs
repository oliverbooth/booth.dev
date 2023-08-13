using OliverBooth.Markdown.Template;

namespace OliverBooth.Services;

/// <summary>
///     Represents a service that renders MediaWiki-style templates.
/// </summary>
public interface ITemplateService
{
    /// <summary>
    ///     Renders the specified template with the specified arguments.
    /// </summary>
    /// <param name="templateInline">The template to render.</param>
    /// <returns>The rendered template.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="templateInline" /> is <see langword="null" />.
    /// </exception>
    string RenderTemplate(TemplateInline templateInline);
}
