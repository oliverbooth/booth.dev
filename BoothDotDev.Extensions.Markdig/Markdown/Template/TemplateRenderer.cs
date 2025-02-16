using BoothDotDev.Extensions.Markdig.Services;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BoothDotDev.Extensions.Markdig.Markdown.Template;

/// <summary>
///     Represents a Markdown object renderer that handles <see cref="TemplateInline" /> elements.
/// </summary>
internal sealed class TemplateRenderer : HtmlObjectRenderer<TemplateInline>
{
    private readonly MarkdownPipeline _pipeline;
    private readonly ITemplateService _templateService;
    private readonly ILogger<TemplateRenderer> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateRenderer" /> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="pipeline">The Markdown pipeline.</param>
    /// <param name="templateService">The <see cref="ITemplateService" />.</param>
    public TemplateRenderer(IServiceProvider serviceProvider, MarkdownPipeline pipeline, ITemplateService templateService)
    {
        _logger = serviceProvider.GetRequiredService<ILogger<TemplateRenderer>>();
        _pipeline = pipeline;
        _templateService = templateService;
    }

    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, TemplateInline template)
    {
        if (template.Name != "Embed")
        {
            renderer.Write(_templateService.RenderGlobalTemplate(template));
            return;
        }

        string filename = $"data/embeds/{template.ArgumentList[0]}";
        if (!File.Exists(filename))
        {
            _logger.LogWarning("Embed file {Filename} does not exist", filename);
            return;
        }

        if (Path.GetExtension(filename) == ".html")
        {
            _logger.LogDebug("Embedding HTML file {Filename}", filename);
            renderer.Write(File.ReadAllText(filename));
        }
        else if (Path.GetExtension(filename) == ".md")
        {
            _logger.LogDebug("Embedding Markdown file {Filename}", filename);
            string markdown = File.ReadAllText(filename);
            string html = global::Markdig.Markdown.ToHtml(markdown, _pipeline);
            renderer.Write(html);
        }
    }
}
