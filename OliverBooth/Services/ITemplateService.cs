using OliverBooth.Data;
using OliverBooth.Markdown.Template;

namespace OliverBooth.Services;

/// <summary>
///     Represents a service that renders MediaWiki-style templates.
/// </summary>
public interface ITemplateService
{
    /// <summary>
    ///     Renders the specified global template with the specified arguments.
    /// </summary>
    /// <param name="templateInline">The global template to render.</param>
    /// <returns>The rendered global template.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="templateInline" /> is <see langword="null" />.
    /// </exception>
    string RenderGlobalTemplate(TemplateInline templateInline);

    /// <summary>
    ///     Renders the specified global template with the specified arguments.
    /// </summary>
    /// <param name="templateInline">The global template to render.</param>
    /// <param name="template">The database template object.</param>
    /// <returns>The rendered global template.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="templateInline" /> is <see langword="null" />.
    /// </exception>
    string RenderTemplate(TemplateInline templateInline, ITemplate? template);
}
