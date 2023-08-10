using Humanizer;
using Microsoft.AspNetCore.Mvc;
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
        if (!ValidateReferer()) return NotFound();
        return Ok(new { count = _blogService.AllPosts.Count });
    }

    [Route("all/{skip:int?}/{take:int?}")]
    public IActionResult GetAllBlogPosts(int skip = 0, int take = -1)
    {
        if (!ValidateReferer()) return NotFound();
        if (take == -1) take = _blogService.AllPosts.Count;
        return Ok(_blogService.AllPosts.Skip(skip).Take(take).Select(post => new
        {
            id = post.Id,
            commentsEnabled = post.EnableComments,
            identifier = post.GetDisqusIdentifier(),
            author = post.AuthorId,
            title = post.Title,
            published = post.Published.ToUnixTimeSeconds(),
            formattedDate = post.Published.ToString("dddd, d MMMM yyyy HH:mm"),
            updated = post.Updated?.ToUnixTimeSeconds(),
            humanizedTimestamp = post.Updated?.Humanize() ?? post.Published.Humanize(),
            excerpt = _blogService.GetExcerpt(post, out bool trimmed),
            trimmed,
            url = Url.Page("/Blog/Article",
                new
                {
                    year = post.Published.ToString("yyyy"),
                    month = post.Published.ToString("MM"),
                    day = post.Published.ToString("dd"),
                    slug = post.Slug
                })
        }));
    }

    [Route("author/{id:int}")]
    public IActionResult GetAuthor(int id)
    {
        if (!ValidateReferer()) return NotFound();
        if (!_blogService.TryGetAuthor(id, out Author? author)) return NotFound();

        return Ok(new
        {
            name = author.Name,
            avatarHash = author.AvatarHash,
        });
    }
    
    private bool ValidateReferer()
    {
        var referer = Request.Headers["Referer"].ToString();
        return referer.StartsWith(Url.PageLink("/Blog/Index")!);
    }
}
