using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using OliverBooth.Data.Blog;
using OliverBooth.Data.Rss;
using OliverBooth.Services;

namespace OliverBooth.Middleware;

/// <summary>
///     Represents the RSS middleware.
/// </summary>
internal sealed class RssMiddleware
{
    private readonly BlogService _blogService;
    private readonly ConfigurationService _configurationService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RssMiddleware" /> class.
    /// </summary>
    /// <param name="_">The request delegate.</param>
    /// <param name="blogService">The blog service.</param>
    /// <param name="configurationService">The configuration service.</param>
    public RssMiddleware(RequestDelegate _,
        BlogService blogService,
        ConfigurationService configurationService)
    {
        _blogService = blogService;
        _configurationService = configurationService;
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Middleware")]
    public async Task Invoke(HttpContext context)
    {
        context.Response.ContentType = "application/rss+xml";

        var baseUrl = $"https://{context.Request.Host}/blog";
        var blogItems = new List<BlogItem>();

        foreach (BlogPost blogPost in _blogService.AllPosts.OrderByDescending(p => p.Published))
        {
            var url = $"{baseUrl}/{blogPost.Published:yyyy/MM/dd}/{blogPost.Slug}";
            string excerpt = _blogService.GetExcerpt(blogPost, out _);
            var description = $"{excerpt}<p><a href=\"{url}\">Read more...</a></p>";

            _blogService.TryGetAuthor(blogPost, out Author? author);

            var item = new BlogItem
            {
                Title = blogPost.Title,
                Link = url,
                Comments = $"{url}#disqus_thread",
                Creator = author?.Name ?? string.Empty,
                PubDate = blogPost.Published.ToString("R"),
                Guid = $"{baseUrl}?pid={blogPost.Id}",
                Description = description
            };
            blogItems.Add(item);
        }

        string siteTitle = _configurationService.GetSiteConfiguration("SiteTitle") ?? string.Empty;
        var rss = new BlogRoot
        {
            Channel = new BlogChannel
            {
                AtomLink = new AtomLink
                {
                    Href = $"{baseUrl}/feed/",
                },
                Description = $"{baseUrl}/",
                LastBuildDate = DateTimeOffset.UtcNow.ToString("R"),
                Link = $"{baseUrl}/",
                Title = siteTitle,
                Generator = $"{baseUrl}/",
                Items = blogItems
            }
        };

        var serializer = new XmlSerializer(typeof(BlogRoot));
        var xmlNamespaces = new XmlSerializerNamespaces();
        xmlNamespaces.Add("content", "http://purl.org/rss/1.0/modules/content/");
        xmlNamespaces.Add("wfw", "http://wellformedweb.org/CommentAPI/");
        xmlNamespaces.Add("dc", "http://purl.org/dc/elements/1.1/");
        xmlNamespaces.Add("atom", "http://www.w3.org/2005/Atom");
        xmlNamespaces.Add("sy", "http://purl.org/rss/1.0/modules/syndication/");
        xmlNamespaces.Add("slash", "http://purl.org/rss/1.0/modules/slash/");

        await using var writer = new StreamWriter(context.Response.BodyWriter.AsStream());
        serializer.Serialize(writer, rss, xmlNamespaces);
        // await context.Response.WriteAsync(document.OuterXml);
    }
}
