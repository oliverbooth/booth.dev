using System.Xml.Serialization;
using BoothDotDev.Common.Data.Blog;
using BoothDotDev.Common.Services;
using BoothDotDev.Data.Blog.Rss;
using Microsoft.AspNetCore.Mvc;

namespace BoothDotDev.Controllers.Blog;

[ApiController]
[Route("blog/feed")]
internal sealed class RssController : Controller
{
    private readonly IBlogPostService _blogPostService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RssController" /> class.
    /// </summary>
    /// <param name="blogPostService">The <see cref="IBlogPostService" />.</param>
    public RssController(IBlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    [HttpGet]
    [Produces("application/xml")]
    public IActionResult OnGet()
    {
        Response.ContentType = "application/xml";

        var baseUrl = $"https://{Request.Host}/blog";
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
                Guid = post.WordPressId.HasValue ? $"{baseUrl}?p={post.WordPressId.Value}" : $"{baseUrl}?pid={post.Id}",
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

        using var writer = new StreamWriter(Response.BodyWriter.AsStream());
        serializer.Serialize(writer, rss, xmlNamespaces);

        return Ok();
    }
}
