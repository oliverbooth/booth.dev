using Humanizer;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OliverBooth.Data.Blog;
using OliverBooth.Services;

namespace OliverBooth.Controllers;

/// <summary>
///     Represents a controller for the blog API.
/// </summary>
[ApiController]
[Route("api/blog")]
[Produces("application/json")]
[EnableCors("BlogApi")]
public sealed class BlogApiController : ControllerBase
{
    private readonly BlogService _blogService;
    private readonly BlogUserService _blogUserService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogApiController" /> class.
    /// </summary>
    /// <param name="blogService">The <see cref="BlogService" />.</param>
    /// <param name="blogUserService">The <see cref="BlogUserService" />.</param>
    public BlogApiController(BlogService blogService, BlogUserService blogUserService)
    {
        _blogService = blogService;
        _blogUserService = blogUserService;
    }

    [Route("count")]
    public IActionResult Count()
    {
        if (!ValidateReferer()) return NotFound();
        return Ok(new { count = _blogService.AllPosts.Count });
    }

    [HttpGet("all/{skip:int?}/{take:int?}")]
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
            url = Url.Page("/Article",
                new
                {
                    area = "blog",
                    year = post.Published.ToString("yyyy"),
                    month = post.Published.ToString("MM"),
                    day = post.Published.ToString("dd"),
                    slug = post.Slug
                })
        }));
    }

    [HttpGet("author/{id:guid}")]
    public IActionResult GetAuthor(Guid id)
    {
        if (!ValidateReferer()) return NotFound();
        if (!_blogUserService.TryGetUser(id, out User? author)) return NotFound();

        return Ok(new
        {
            id = author.Id,
            name = author.DisplayName,
            avatarHash = author.AvatarHash,
        });
    }

    private bool ValidateReferer()
    {
        var referer = Request.Headers["Referer"].ToString();
        return referer.StartsWith(Url.PageLink("/index", values: new { area = "blog" })!);
    }
}
