using Markdig;
using OliverBooth.Extensions.Markdig.Markdown.Template;
using OliverBooth.Extensions.Markdig.Services;

namespace OliverBooth.Extensions.Markdig;

/// <summary>
///     Extension methods for <see cref="MarkdownPipelineBuilder" />.
/// </summary>
public static class MarkdownPipelineExtensions
{
    /// <summary>
    ///     Enables the use of Wiki-style templates in this pipeline.
    /// </summary>
    /// <param name="builder">The Markdig markdown pipeline builder.</param>
    /// <param name="templateService">The template service responsible for fetching and rendering templates.</param>
    /// <returns>The modified Markdig markdown pipeline builder.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="builder" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="templateService" /> is <see langword="null" />.</para>
    /// </exception>
    public static MarkdownPipelineBuilder UseTemplates(this MarkdownPipelineBuilder builder, ITemplateService templateService)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (templateService is null)
        {
            throw new ArgumentNullException(nameof(templateService));
        }

        builder.Use(new TemplateExtension(templateService));
        return builder;
    }
}
