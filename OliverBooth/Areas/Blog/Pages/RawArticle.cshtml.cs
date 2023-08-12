using Cysharp.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OliverBooth.Data.Blog;
using OliverBooth.Services;

namespace OliverBooth.Areas.Blog.Pages;

/// <summary>
///     Represents the page model for the <c>RawArticle</c> page.
/// </summary>
[Area("blog")]
public class RawArticle : PageModel
{
    private readonly BlogService _blogService;
    private readonly BlogUserService _blogUserService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RawArticle" /> class.
    /// </summary>
    /// <param name="blogService">The <see cref="BlogService" />.</param>
    /// <param name="blogUserService">The <see cref="BlogUserService" />.</param>
    public RawArticle(BlogService blogService, BlogUserService blogUserService)
    {
        _blogService = blogService;
        _blogUserService = blogUserService;
    }

    public IActionResult OnGet(int year, int month, int day, string slug)
    {
        if (!_blogService.TryGetBlogPost(year, month, day, slug, out BlogPost? post))
        {
            return NotFound();
        }

        Response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        using Utf8ValueStringBuilder builder = ZString.CreateUtf8StringBuilder();
        builder.AppendLine("# " + post.Title);
        if (_blogUserService.TryGetUser(post.AuthorId, out User? author))
            builder.AppendLine($"Author: {author.DisplayName}");

        builder.AppendLine($"Published: {post.Published:R}");
        if (post.Updated.HasValue)
            builder.AppendLine($"Updated: {post.Updated:R}");

        builder.AppendLine();
        builder.AppendLine(post.Body);
        return Content(builder.ToString());
    }
}
