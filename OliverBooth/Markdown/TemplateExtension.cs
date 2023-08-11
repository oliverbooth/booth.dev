using Markdig;
using Markdig.Renderers;
using OliverBooth.Services;

namespace OliverBooth.Markdown;

/// <summary>
///     Represents a Markdown extension that adds support for MediaWiki-style templates.
/// </summary>
internal sealed class TemplateExtension : IMarkdownExtension
{
    private readonly TemplateService _templateService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateExtension" /> class.
    /// </summary>
    /// <param name="templateService">The template service.</param>
    public TemplateExtension(TemplateService templateService)
    {
        _templateService = templateService;
    }

    /// <inheritdoc />
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        pipeline.InlineParsers.AddIfNotAlready<TemplateInlineParser>();
    }

    /// <inheritdoc />
    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer)
        {
            htmlRenderer.ObjectRenderers.Add(new TemplateRenderer(_templateService));
        }
    }
}
