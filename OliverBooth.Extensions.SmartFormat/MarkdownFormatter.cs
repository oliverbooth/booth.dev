using Markdig;
using Microsoft.Extensions.DependencyInjection;
using SmartFormat.Core.Extensions;

namespace OliverBooth.Extensions.SmartFormat;

/// <summary>
///     Represents a SmartFormat formatter that formats markdown.
/// </summary>
public sealed class MarkdownFormatter : IFormatter
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MarkdownFormatter" /> class.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider" />.</param>
    public MarkdownFormatter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public bool CanAutoDetect { get; set; } = true;

    /// <inheritdoc />
    public string Name { get; set; } = "markdown";

    /// <inheritdoc />
    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        if (formattingInfo.CurrentValue is not string value)
            return false;

        var pipeline = _serviceProvider.GetService<MarkdownPipeline>();
        formattingInfo.Write(Markdig.Markdown.ToHtml(value, pipeline));
        return true;
    }
}
