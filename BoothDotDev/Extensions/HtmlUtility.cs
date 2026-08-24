using System.Web;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Cysharp.Text;

namespace BoothDotDev.Extensions;

/// <summary>
///     Provides helper methods for generating HTML tags
/// </summary>
public static class HtmlUtility
{
    /// <summary>
    ///     Creates the full set of <c>&lt;meta&gt;</c> embed tags for a page, including a type-appropriate title,
    ///     description, and Open Graph image - for content that has none of its own, falls back to
    ///     <paramref name="fallbackTitle" />/<paramref name="fallbackDescription" /> and the generic site card.
    /// </summary>
    /// <param name="content">
    ///     The page's content, as set in <c>ViewData["Post"]</c> - a <see cref="BlogPost" />, <see cref="TutorialArticle" />,
    ///     <see cref="DevChallenge" />, <see cref="Note" />, <see cref="Project" />, <see cref="ProjectDevlog" />,
    ///     <see cref="ArtworkItem" />, <see cref="MusicItem" />, or <see langword="null" /> for a non-content page.
    /// </param>
    /// <param name="siteBaseUrl">The site's own base URL, used to build an absolute Open Graph image URL.</param>
    /// <param name="markdownRenderingService">The <see cref="MarkdownRenderingService" /> injected by the page.</param>
    /// <param name="fallbackTitle">The title to use when <paramref name="content" /> is <see langword="null" />.</param>
    /// <param name="fallbackDescription">The description to use when <paramref name="content" /> is <see langword="null" />.</param>
    /// <returns>A string containing a collection of <c>&lt;meta&gt;</c> embed tags.</returns>
    public static string CreateMetaTagsForContent(
        object? content,
        Uri siteBaseUrl,
        MarkdownRenderingService markdownRenderingService,
        string fallbackTitle,
        string fallbackDescription)
    {
        return content switch
        {
            BlogPost post => CreateMetaTags(new Dictionary<string, string>
            {
                ["title"] = post.Title,
                ["description"] = markdownRenderingService.RenderPlainTextExcerpt(post, out _).Trim(),
                ["author"] = post.Author.DisplayName,
                ["image"] = OgImageUrl(siteBaseUrl, "blog", post.Id)
            }),
            TutorialArticle article => CreateMetaTags(new Dictionary<string, string>
            {
                ["title"] = article.Title,
                ["description"] = markdownRenderingService.RenderPlainTextExcerpt(article, out _).Trim(),
                ["author"] = Strings.MyName,
                ["image"] = OgImageUrl(siteBaseUrl, "tutorial", article.Id)
            }),
            DevChallenge challenge => CreateMetaTags(new Dictionary<string, string>
            {
                ["title"] = challenge.Title,
                ["description"] = markdownRenderingService.RenderPlainTextPreview(challenge.Description),
                ["author"] = Strings.MyName,
                ["image"] = OgImageUrl(siteBaseUrl, "challenge", challenge.Id)
            }),
            Note note => CreateMetaTags(new Dictionary<string, string>
            {
                ["title"] = note.Title,
                ["description"] = markdownRenderingService.RenderPlainTextPreview(note.Content),
                ["author"] = Strings.MyName,
                ["image"] = OgImageUrl(siteBaseUrl, "note", note.Id)
            }),
            ProjectDevlog devlog => CreateMetaTags(new Dictionary<string, string>
            {
                ["title"] = devlog.Title,
                ["description"] = markdownRenderingService.RenderPlainTextPreview(devlog.Body),
                ["author"] = Strings.MyName,
                ["image"] = OgImageUrl(siteBaseUrl, "devlog", devlog.Id)
            }),
            Project project => CreateMetaTags(new Dictionary<string, string>
            {
                ["title"] = project.Name,
                ["description"] = markdownRenderingService.RenderPlainTextPreview(project.Description),
                ["author"] = Strings.MyName,
                ["image"] = OgImageUrl(siteBaseUrl, "project", project.Id)
            }),
            ArtworkItem artwork => CreateCreationMetaTags(artwork, siteBaseUrl, "artwork", markdownRenderingService),
            MusicItem music => CreateCreationMetaTags(music, siteBaseUrl, "music", markdownRenderingService),
            _ => CreateMetaTags(new Dictionary<string, string>
            {
                ["title"] = fallbackTitle,
                ["description"] = fallbackDescription,
                ["image"] = new Uri(siteBaseUrl, "/og/site.png").ToString()
            })
        };
    }

    private static string CreateCreationMetaTags(
        CreativeItem item, Uri siteBaseUrl, string type, MarkdownRenderingService markdownRenderingService)
    {
        var tags = new Dictionary<string, string>
        {
            ["title"] = item.Title,
            ["author"] = Strings.MyName,
            ["image"] = OgImageUrl(siteBaseUrl, type, item.Id)
        };

        if (!string.IsNullOrWhiteSpace(item.Description))
        {
            tags["description"] = markdownRenderingService.RenderPlainTextPreview(item.Description);
        }

        return CreateMetaTags(tags);
    }

    private static string OgImageUrl(Uri siteBaseUrl, string type, Guid id)
    {
        return new Uri(siteBaseUrl, $"/og/{type}/{id:N}.png").ToString();
    }

    /// <summary>
    ///     Creates <c>&lt;meta&gt;</c> embed tags by pulling data from the specified dictionary.
    /// </summary>
    /// <param name="tags">
    ///     A dictionary containing the tag values. This dictionary should be in the form:
    ///
    ///     <list type="table">
    ///         <listheader>
    ///             <term>Key</term>
    ///             <description>Description</description>
    ///         </listheader>
    ///
    ///         <item>
    ///             <term>description</term>
    ///             <description>
    ///                 The value to apply to the <c>description</c>, <c>og:description</c>, and <c>twitter:description</c>, tags.
    ///             </description>
    ///         </item>
    ///
    ///         <item>
    ///             <term>author</term>
    ///             <description>The value to apply to the <c>og:site_name</c>, and <c>twitter:creator</c>, tags.</description>
    ///         </item>
    ///
    ///         <item>
    ///             <term>title</term>
    ///             <description>
    ///                 The value to apply to the <c>title</c>, <c>og:title</c>, and <c>twitter:title</c>, tags.
    ///             </description>
    ///         </item>
    ///
    ///         <item>
    ///             <term>image</term>
    ///             <description>
    ///                 The absolute URL to apply to the <c>og:image</c> and <c>twitter:image</c> tags.
    ///             </description>
    ///         </item>
    ///     </list>
    ///
    ///     Any other values contained with the dictionary are ignored.
    /// </param>
    /// <returns>A string containing a collection of <c>&lt;meta&gt;</c> embed tags.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tags" /> is <see langword="null" />.</exception>
    public static string CreateMetaTags(IReadOnlyDictionary<string, string> tags)
    {
        if (tags is null)
        {
            throw new ArgumentNullException(nameof(tags));
        }

        using Utf8ValueStringBuilder builder = ZString.CreateUtf8StringBuilder();
        builder.AppendLine("""<meta property="og:type" content="article">""");

        if (tags.TryGetValue("description", out var description))
        {
            description = HttpUtility.HtmlEncode(description);
            builder.AppendLine($"""<meta name="description" content="{description}">""");
            builder.AppendLine($"""<meta property="og:description" content="{description}">""");
            builder.AppendLine($"""<meta property="twitter:description" content="{description}">""");
        }

        if (tags.TryGetValue("author", out var author))
        {
            author = HttpUtility.HtmlEncode(author);
            builder.AppendLine($"""<meta property="og:site_name" content="{author}">""");
            builder.AppendLine($"""<meta property="twitter:creator" content="{author}">""");
        }

        if (tags.TryGetValue("title", out var title))
        {
            title = HttpUtility.HtmlEncode(title);
            builder.AppendLine($"""<meta name="title" content="{title}">""");
            builder.AppendLine($"""<meta property="og:title" content="{title}">""");
            builder.AppendLine($"""<meta property="twitter:title" content="{title}">""");
        }

        if (tags.TryGetValue("image", out var image))
        {
            image = HttpUtility.HtmlEncode(image);
            builder.AppendLine($"""<meta property="og:image" content="{image}">""");
            builder.AppendLine($"""<meta property="og:image:width" content="{OgImageService.Width}">""");
            builder.AppendLine($"""<meta property="og:image:height" content="{OgImageService.Height}">""");
            builder.AppendLine($"""<meta property="twitter:image" content="{image}">""");

            // Without this, Discord/Twitter default to the small "summary" thumbnail layout instead of showing the
            // card full-width - the image tags alone aren't enough to opt into the large-image treatment.
            builder.AppendLine("""<meta name="twitter:card" content="summary_large_image">""");
        }

        return builder.ToString();
    }
}
