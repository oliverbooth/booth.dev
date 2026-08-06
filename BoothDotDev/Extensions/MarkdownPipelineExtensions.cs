using BoothDotDev.Markdown.Callout;
using BoothDotDev.Markdown.Template;
using Markdig;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extension methods for <see cref="MarkdownPipelineBuilder" />.
/// </summary>
public static class MarkdownPipelineExtensions
{
    /// <param name="builder">The Markdig markdown pipeline builder.</param>
    extension(MarkdownPipelineBuilder builder)
    {
        /// <summary>
        ///     Enables the use of Obsidian-style callouts in this pipeline.
        /// </summary>
        /// <returns>The modified Markdig markdown pipeline builder.</returns>
        public MarkdownPipelineBuilder UseCallouts()
        {
            builder.Extensions.AddIfNotAlready<CalloutExtension>();
            return builder;
        }

        /// <summary>
        ///     Enables the use of Wiki-style templates in this pipeline.
        /// </summary>
        /// <param name="serviceProvider">The service provider responsible for fetching services.</param>
        /// <returns>The modified Markdig markdown pipeline builder.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="serviceProvider" /> is <see langword="null" />.</exception>
        public MarkdownPipelineBuilder UseTemplates(IServiceProvider serviceProvider)
        {
            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            builder.Use(new TemplateExtension(serviceProvider));
            return builder;
        }
    }
}
