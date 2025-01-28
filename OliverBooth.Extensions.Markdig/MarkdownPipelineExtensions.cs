using Markdig;
using OliverBooth.Extensions.Markdig.Markdown.Callout;
using OliverBooth.Extensions.Markdig.Markdown.Template;

namespace OliverBooth.Extensions.Markdig;

/// <summary>
///     Extension methods for <see cref="MarkdownPipelineBuilder" />.
/// </summary>
public static class MarkdownPipelineExtensions
{
    /// <summary>
    ///     Enables the use of Obsidian-style callouts in this pipeline.
    /// </summary>
    /// <param name="builder">The Markdig markdown pipeline builder.</param>
    /// <returns>The modified Markdig markdown pipeline builder.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder" /> is <see langword="null" />.</exception>
    public static MarkdownPipelineBuilder UseCallouts(this MarkdownPipelineBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Extensions.AddIfNotAlready<CalloutExtension>();
        return builder;
    }

    /// <summary>
    ///     Enables the use of Wiki-style templates in this pipeline.
    /// </summary>
    /// <param name="builder">The Markdig markdown pipeline builder.</param>
    /// <param name="serviceProvider">The service provider responsible for fetching services.</param>
    /// <returns>The modified Markdig markdown pipeline builder.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="builder" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="serviceProvider" /> is <see langword="null" />.</para>
    /// </exception>
    public static MarkdownPipelineBuilder UseTemplates(this MarkdownPipelineBuilder builder, IServiceProvider serviceProvider)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (serviceProvider is null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

        builder.Use(new TemplateExtension(serviceProvider));
        return builder;
    }
}
