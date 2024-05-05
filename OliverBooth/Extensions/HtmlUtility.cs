using System.Web;
using Cysharp.Text;
using OliverBooth.Common.Data.Blog;
using OliverBooth.Common.Data.Web;
using OliverBooth.Common.Services;

namespace OliverBooth.Extensions;

/// <summary>
///     Provides helper methods for generating HTML tags 
/// </summary>
public static class HtmlUtility
{
    /// <summary>
    ///     Creates <c>&lt;meta&gt;</c> embed tags by pulling data from the specified blog post.
    /// </summary>
    /// <param name="post">The blog post whose metadata should be retrieved.</param>
    /// <param name="blogPostService">The <see cref="IBlogPostService" /> injected by the page.</param>
    /// <returns>A string containing a collection of <c>&lt;meta&gt;</c> embed tags.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="post" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="blogPostService" /> is <see langword="null" />.</para>
    /// </exception>
    public static string CreateMetaTagsFromPost(IBlogPost post, IBlogPostService blogPostService)
    {
        if (post is null)
        {
            throw new ArgumentNullException(nameof(post));
        }

        if (blogPostService is null)
        {
            throw new ArgumentNullException(nameof(blogPostService));
        }


        string excerpt = blogPostService.RenderExcerpt(post, out _);
        var tags = new Dictionary<string, string>
        {
            ["title"] = post.Title,
            ["description"] = excerpt,
            ["author"] = post.Author.DisplayName
        };
        return CreateMetaTags(tags);
    }

    /// <summary>
    ///     Creates <c>&lt;meta&gt;</c> embed tags by pulling data from the specified article.
    /// </summary>
    /// <param name="article">The article whose metadata should be retrieved.</param>
    /// <param name="tutorialService">The <see cref="ITutorialService" /> injected by the page.</param>
    /// <returns>A string containing a collection of <c>&lt;meta&gt;</c> embed tags.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="article" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="tutorialService" /> is <see langword="null" />.</para>
    /// </exception>
    public static string CreateMetaTagsFromTutorialArticle(ITutorialArticle article, ITutorialService tutorialService)
    {
        if (article is null)
        {
            throw new ArgumentNullException(nameof(article));
        }

        if (tutorialService is null)
        {
            throw new ArgumentNullException(nameof(tutorialService));
        }


        string excerpt = tutorialService.RenderExcerpt(article, out _);
        var tags = new Dictionary<string, string>
        {
            ["title"] = article.Title,
            ["description"] = excerpt,
            ["author"] = "Oliver Booth" // TODO add article author support?
        };
        return CreateMetaTags(tags);
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

        if (tags.TryGetValue("description", out string? description))
        {
            description = HttpUtility.HtmlEncode(description);
            builder.AppendLine($"""<meta name="description" content="{description}">""");
            builder.AppendLine($"""<meta property="og:description" content="{description}">""");
            builder.AppendLine($"""<meta property="twitter:description" content="{description}">""");
        }

        if (tags.TryGetValue("author", out string? author))
        {
            author = HttpUtility.HtmlEncode(author);
            builder.AppendLine($"""<meta property="og:site_name" content="{author}">""");
            builder.AppendLine($"""<meta property="twitter:creator" content="{author}">""");
        }

        if (tags.TryGetValue("title", out string? title))
        {
            title = HttpUtility.HtmlEncode(title);
            builder.AppendLine($"""<meta name="title" content="{title}">""");
            builder.AppendLine($"""<meta property="og:title" content="{title}">""");
            builder.AppendLine($"""<meta property="twitter:title" content="{title}">""");
        }

        return builder.ToString();
    }
}
