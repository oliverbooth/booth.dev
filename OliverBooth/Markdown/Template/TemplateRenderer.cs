using Markdig.Renderers;
using Markdig.Renderers.Html;
using OliverBooth.Services;

namespace OliverBooth.Markdown.Template;

/// <summary>
///     Represents a Markdown object renderer that handles <see cref="TemplateInline" /> elements.
/// </summary>
internal sealed class TemplateRenderer : HtmlObjectRenderer<TemplateInline>
{
    private readonly ITemplateService _templateService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateRenderer" /> class.
    /// </summary>
    /// <param name="templateService">The <see cref="TemplateService" />.</param>
    public TemplateRenderer(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, TemplateInline template)
    {
        renderer.Write(_templateService.RenderGlobalTemplate(template));
    }
}
