using Asp.Versioning;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using OliverBooth.Common.Data.Blog;
using OliverBooth.Common.Data.Web.Users;
using OliverBooth.Common.Services;
using OliverBooth.Services;

namespace OliverBooth.Controllers.Api.v1;

/// <summary>
///     Represents a controller for the blog API.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/blog")]
[Produces("application/json")]
[ApiVersion(1)]
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
        return Ok(new { count = _blogPostService.GetBlogPostCount() });
    }

    [HttpGet("posts/{page:int?}")]
    public IActionResult GetAllBlogPosts(int page = 0)
    {
        const int itemsPerPage = 10;
        IReadOnlyList<IBlogPost> allPosts = _blogPostService.GetBlogPosts(page, itemsPerPage);
        return Ok(allPosts.Select(post => CreatePostObject(post)));
    }

    [HttpGet("posts/tagged/{tag}/{page:int?}")]
    public IActionResult GetTaggedBlogPosts(string tag, int page = 0)
    {
        const int itemsPerPage = 10;
        tag = tag.Replace('-', ' ').ToLowerInvariant();

        IReadOnlyList<IBlogPost> allPosts = _blogPostService.GetBlogPosts(page, itemsPerPage);
        allPosts = allPosts.Where(post => post.Tags.Contains(tag)).ToList();
        return Ok(allPosts.Select(post => CreatePostObject(post)));
    }

    [HttpGet("author/{id:guid}")]
    public IActionResult GetAuthor(Guid id)
    {
        if (!_userService.TryGetUser(id, out IUser? author))
        {
            return NotFound();
        }

        return Ok(new
        {
            id = author.Id,
            name = author.DisplayName,
            avatarUrl = author.AvatarUrl,
        });
    }

    [HttpGet("post/{id:guid?}")]
    public IActionResult GetPost(Guid id)
    {
        if (!_blogPostService.TryGetPost(id, out IBlogPost? post))
        {
            return NotFound();
        }

        return Ok(CreatePostObject(post, true));
    }

    private object CreatePostObject(IBlogPost post, bool includeContent = false)
    {
        return new
        {
            id = post.Id,
            commentsEnabled = post.EnableComments,
            identifier = post.GetDisqusIdentifier(),
            author = post.Author.Id,
            title = post.Title,
            published = post.Published.ToUnixTimeSeconds(),
            updated = post.Updated?.ToUnixTimeSeconds(),
            formattedPublishDate = post.Published.ToString("dddd, d MMMM yyyy HH:mm"),
            formattedUpdateDate = post.Updated?.ToString("dddd, d MMMM yyyy HH:mm"),
            humanizedTimestamp = post.Updated?.Humanize() ?? post.Published.Humanize(),
            excerpt = _blogPostService.RenderExcerpt(post, out bool trimmed),
            content = includeContent ? _blogPostService.RenderPost(post) : null,
            trimmed,
            tags = post.Tags.Select(t => t.Replace(' ', '-')),
            url = new
            {
                year = post.Published.ToString("yyyy"),
                month = post.Published.ToString("MM"),
                day = post.Published.ToString("dd"),
                slug = post.Slug
            }
        };
    }
}
