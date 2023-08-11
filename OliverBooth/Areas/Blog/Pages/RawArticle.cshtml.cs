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

    /// <summary>
    ///     Initializes a new instance of the <see cref="RawArticle" /> class.
    /// </summary>
    /// <param name="blogService">The <see cref="BlogService" />.</param>
    public RawArticle(BlogService blogService)
    {
        _blogService = blogService;
    }

    public IActionResult OnGet(int year, int month, int day, string slug)
    {
        if (!_blogService.TryGetBlogPost(year, month, day, slug, out BlogPost? post))
        {
            Response.StatusCode = 404;
            return NotFound();
        }
        
        using Utf8ValueStringBuilder builder = ZString.CreateUtf8StringBuilder();
        builder.AppendLine("# " + post.Title);
        if (_blogService.TryGetAuthor(post, out Author? author))
            builder.AppendLine($"Author: {author.Name}");
        
        builder.AppendLine($"Published: {post.Published:R}");
        if (post.Updated.HasValue)
            builder.AppendLine($"Updated: {post.Updated:R}");
        
        builder.AppendLine();
        builder.AppendLine(post.Body);
        return Content(builder.ToString(), "text/plain");
    }
}
