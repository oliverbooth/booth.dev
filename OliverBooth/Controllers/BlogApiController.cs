using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using OliverBooth.Data.Blog;
using OliverBooth.Services;

namespace OliverBooth.Controllers;

[Controller]
[Route("/api/blog")]
public sealed class BlogApiController : ControllerBase
{
    private readonly BlogService _blogService;

    public BlogApiController(BlogService blogService)
    {
        _blogService = blogService;
    }

    [Route("count")]
    public IActionResult Count()
    {
        return new JsonResult(new { count = _blogService.AllPosts.Count });
    }

    [Route("all/{skip:int?}/{take:int?}")]
    public IActionResult GetAllBlogPosts(int skip = 0, int take = -1)
    {
        if (take == -1) take = _blogService.AllPosts.Count;

        var referer = Request.Headers["Referer"].ToString();
        Console.WriteLine($"Referer: {referer}");
        if (!referer.StartsWith(Url.PageLink("/Blog/Index")!))
        {
            return NotFound();
        }

        return new JsonResult(_blogService.AllPosts.Skip(skip).Take(take).Select(post => new
        {
            id = post.Id,
            commentsEnabled = post.EnableComments,
            identifier = post.GetDisqusIdentifier(),
            author = post.AuthorId,
            title = post.Title,
            published = post.Published.ToUnixTimeSeconds(),
            updated = post.Updated?.ToUnixTimeSeconds(),
            excerpt = _blogService.GetExcerpt(post, out bool trimmed),
            trimmed,
            url = Url.Page("/Blog/Article",
                new
                {
                    year = post.Published.Year,
                    month = post.Published.Month,
                    day = post.Published.Day,
                    slug = post.Slug
                })
        }));
    }

    [Route("author/{id:int}")]
    public IActionResult GetAuthor(int id)
    {
        if (!_blogService.TryGetAuthor(id, out Author? author)) return NotFound();

        return new JsonResult(new
        {
            name = author.Name,
            avatarHash = author.AvatarHash,
        });
    }
}
