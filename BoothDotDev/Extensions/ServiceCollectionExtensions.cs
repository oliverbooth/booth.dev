using BoothDotDev.Extensions.Markdig;
using BoothDotDev.Extensions.Markdig.Markdown.Timestamp;
using Markdig;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extension methods for <see cref="IServiceCollection" />.
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <param name="services">The <see cref="IServiceCollection" />.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Adds a preconfigured Markdown pipeline to the service collection.
        /// </summary>
        /// <returns>The <see cref="IServiceCollection" />.</returns>
        public IServiceCollection AddMarkdownPipeline()
        {
            return services.AddSingleton(provider => new MarkdownPipelineBuilder()
                .Use<TimestampExtension>()
                .UseTemplates(provider)

                // we have our own "alert blocks" in the form of GitHub and Obsidian style callouts
                .UseCallouts()

                // advanced extensions. add explicitly to avoid UseAlertBlocks
                .UseAbbreviations()
                .UseAutoIdentifiers()
                .UseCitations()
                .UseCustomContainers()
                .UseDefinitionLists()
                .UseEmphasisExtras()
                .UseFigures()
                .UseFooters()
                .UseFootnotes()
                .UseGridTables()
                .UseMathematics()
                .UseMediaLinks()
                .UsePipeTables()
                .UseListExtras()
                .UseTaskLists()
                .UseDiagrams()
                .UseAutoLinks()
                .UseGenericAttributes() // must be last as it is one parser modifying other parsers

                // no more advanced extensions
                .UseBootstrap()
                .UseEmojiAndSmiley()
                .UseSmartyPants()
                .Build());
        }
    }
}
