using BoothDotDev.Services;
using HtmlAgilityPack;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace BoothDotDev.Markdown.Template;

/// <summary>
///     Represents a Markdown object renderer that handles <see cref="TemplateInline" /> elements.
/// </summary>
internal sealed class TemplateRenderer : HtmlObjectRenderer<TemplateInline>
{
    private readonly TemplateService _templateService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateRenderer" /> class.
    /// </summary>
    /// <param name="templateService">The <see cref="TemplateService" />.</param>
    public TemplateRenderer(TemplateService templateService)
    {
        _templateService = templateService;
    }

    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, TemplateInline template)
    {
        var html = _templateService.RenderGlobalTemplate(template);

        if (renderer.EnableHtmlForInline)
        {
            renderer.Write(html);
            return;
        }

        // plain-text renderers (MD.ToPlainText, used for OG descriptions/images) have no markup to fall back on here
        // the way CalloutRenderer falls back to its block's child content - a template's partial view is opaque HTML,
        // not Markdown - so reduce it to its visible text instead of writing the partial's markup verbatim
        var document = new HtmlDocument();
        document.LoadHtml(html);
        renderer.Write(document.DocumentNode.InnerText);
    }
}
