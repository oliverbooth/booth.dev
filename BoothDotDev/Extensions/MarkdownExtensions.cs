using System.Diagnostics.CodeAnalysis;
using BoothDotDev.Markdown.Callout;
using BoothDotDev.Markdown.Embed;
using BoothDotDev.Markdown.Subtext;
using BoothDotDev.Markdown.Template;
using HtmlAgilityPack;
using Markdig;
using MD = Markdig.Markdown;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extension methods for <see cref="Markdig" />.
/// </summary>
public static class MarkdownExtensions
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
        ///     Enables the use of Discord-style subtext (<c>-#</c>) in this pipeline.
        /// </summary>
        /// <returns>The modified Markdig markdown pipeline builder.</returns>
        public MarkdownPipelineBuilder UseSubtext()
        {
            builder.Extensions.AddIfNotAlready<SubtextExtension>();
            return builder;
        }

        /// <summary>
        ///     Enables the use of Obsidian-style file embeds in this pipeline.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The modified Markdig markdown pipeline builder.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="serviceProvider" /> is <see langword="null" />.</exception>
        public MarkdownPipelineBuilder UseEmbeds(IServiceProvider serviceProvider)
        {
            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            builder.Use(new EmbedExtension(serviceProvider));
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

    /// <summary>
    ///     Extension methods <see cref="MD" />.
    /// </summary>
    extension(MD)
    {
        /// <summary>
        ///     Converts the specified Markdown string to HTML and unwraps it if it is wrapped in a single <c>&lt;p&gt;</c> tag.
        /// </summary>
        /// <param name="markdown">The Markdown string to convert.</param>
        /// <param name="pipeline">The Markdig pipeline to use for conversion.</param>
        /// <param name="context">The Markdig parser context.</param>
        /// <returns>The unwrapped HTML string.</returns>
        public static string ToHtmlUnwrapped([StringSyntax("markdown")] string markdown,
            MarkdownPipeline? pipeline = null,
            MarkdownParserContext? context = null)
        {
            var html = MD.ToHtml(markdown, pipeline, context);
            var document = new HtmlDocument();
            document.LoadHtml(html);

            if (document.DocumentNode.FirstChild is { Name: "p" } child)
            {
                return child.InnerHtml;
            }

            return html;
        }
    }
}
