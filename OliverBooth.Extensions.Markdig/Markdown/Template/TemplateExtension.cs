using Markdig;
using Markdig.Renderers;
using Microsoft.Extensions.DependencyInjection;
using OliverBooth.Extensions.Markdig.Services;

namespace OliverBooth.Extensions.Markdig.Markdown.Template;

/// <summary>
///     Represents a Markdown extension that adds support for MediaWiki-style templates.
/// </summary>
public sealed class TemplateExtension : IMarkdownExtension
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITemplateService _templateService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateExtension" /> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public TemplateExtension(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _templateService = serviceProvider.GetRequiredService<ITemplateService>();
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
            htmlRenderer.ObjectRenderers.Add(new TemplateRenderer(_serviceProvider, pipeline, _templateService));
        }
    }
}
