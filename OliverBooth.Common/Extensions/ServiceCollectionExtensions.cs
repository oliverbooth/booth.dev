using Markdig;
using Microsoft.Extensions.DependencyInjection;
using OliverBooth.Common.Markdown;
using OliverBooth.Common.Services;

namespace OliverBooth.Common.Extensions;

/// <summary>
///     Extension methods for <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the Markdown pipeline to the <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" />.</param>
    /// <returns>The <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection AddMarkdownPipeline(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton(provider => new MarkdownPipelineBuilder()
            .Use(new TemplateExtension(provider.GetRequiredService<ITemplateService>()))
            .UseAdvancedExtensions()
            .UseBootstrap()
            .UseEmojiAndSmiley()
            .UseSmartyPants()
            .Build());
    }
}
