using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using OliverBooth.Data.Blog;
using OliverBooth.Data.Blog.Rss;
using OliverBooth.Services;

namespace OliverBooth.Middleware;

internal sealed class RssMiddleware
{
    private readonly IBlogPostService _blogPostService;

    public RssMiddleware(RequestDelegate _, IBlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Middleware")]
    public async Task Invoke(HttpContext context)
    {
        context.Response.ContentType = "application/rss+xml";

        var baseUrl = $"https://{context.Request.Host}/blog";
        var blogItems = new List<BlogItem>();

        foreach (IBlogPost post in _blogPostService.GetAllBlogPosts())
        {
            var url = $"{baseUrl}/{post.Published:yyyy/MM/dd}/{post.Slug}";
            string excerpt = _blogPostService.RenderExcerpt(post, out _);
            var description = $"{excerpt}<p><a href=\"{url}\">Read more...</a></p>";

            var item = new BlogItem
            {
                Title = post.Title,
                Link = url,
                Comments = $"{url}#disqus_thread",
                Creator = post.Author.DisplayName,
                PubDate = post.Published.ToString("R"),
                Guid = $"{baseUrl}?pid={post.Id}",
                Description = description
            };
            blogItems.Add(item);
        }

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
                Title = "Oliver Booth",
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
    }
}
