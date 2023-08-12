using Humanizer;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OliverBooth.Blog.Data;
using OliverBooth.Blog.Services;

namespace OliverBooth.Blog.Controllers;

/// <summary>
///     Represents a controller for the blog API.
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
[EnableCors("OliverBooth")]
public sealed class BlogApiController : ControllerBase
{
    private readonly IBlogPostService _blogPostService;
    private readonly IUserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogApiController" /> class.
    /// </summary>
    /// <param name="blogPostService">The <see cref="IBlogPostService" />.</param>
    /// <param name="userService">The <see cref="IUserService" />.</param>
    public BlogApiController(IBlogPostService blogPostService, IUserService userService)
    {
        _blogPostService = blogPostService;
        _userService = userService;
    }

    [Route("count")]
    public IActionResult Count()
    {
        if (!ValidateReferer()) return NotFound();
        return Ok(new { count = _blogPostService.GetAllBlogPosts().Count });
    }

    [HttpGet("all/{skip:int?}/{take:int?}")]
    public IActionResult GetAllBlogPosts(int skip = 0, int take = -1)
    {
        if (!ValidateReferer()) return NotFound();
        
        // TODO yes I'm aware I can use the new pagination I wrote, this will be added soon.
        IReadOnlyList<IBlogPost> allPosts = _blogPostService.GetAllBlogPosts();

        if (take == -1)
        {
            take = allPosts.Count;
        }

        return Ok(allPosts.Skip(skip).Take(take).Select(post => new
        {
            id = post.Id,
            commentsEnabled = post.EnableComments,
            identifier = post.GetDisqusIdentifier(),
            author = post.Author.Id,
            title = post.Title,
            published = post.Published.ToUnixTimeSeconds(),
            formattedDate = post.Published.ToString("dddd, d MMMM yyyy HH:mm"),
            updated = post.Updated?.ToUnixTimeSeconds(),
            humanizedTimestamp = post.Updated?.Humanize() ?? post.Published.Humanize(),
            excerpt = _blogPostService.RenderExcerpt(post, out bool trimmed),
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
        if (!_userService.TryGetUser(id, out IUser? author)) return NotFound();

        return Ok(new
        {
            id = author.Id,
            name = author.DisplayName,
            avatarUrl = author.AvatarUrl,
        });
    }

    private bool ValidateReferer()
    {
        var referer = Request.Headers["Referer"].ToString();
        return referer.StartsWith(Url.PageLink("/index")!);
    }
}
