using System.Diagnostics.CodeAnalysis;
using OliverBooth.Data.Web;
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

    /// <summary>
    ///     Attempts to get the template with the specified name.
    /// </summary>
    /// <param name="name">The name of the template.</param>
    /// <param name="template">
    ///     When this method returns, contains the template with the specified name, if the template is found;
    ///     otherwise, <see langword="null" />.
    /// </param>
    /// <returns><see langword="true" /> if the template exists; otherwise, <see langword="false" />.</returns>
    bool TryGetTemplate(string name, [NotNullWhen(true)] out ITemplate? template);

    /// <summary>
    ///     Attempts to get the template with the specified name and variant.
    /// </summary>
    /// <param name="name">The name of the template.</param>
    /// <param name="variant">The variant of the template.</param>
    /// <param name="template">
    ///     When this method returns, contains the template with the specified name and variant, if the template is
    ///     found; otherwise, <see langword="null" />.
    /// </param>
    /// <returns><see langword="true" /> if the template exists; otherwise, <see langword="false" />.</returns>
    bool TryGetTemplate(string name, string variant, [NotNullWhen(true)] out ITemplate? template);
}
