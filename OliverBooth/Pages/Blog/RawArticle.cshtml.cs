using Cysharp.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OliverBooth.Data.Blog;
using OliverBooth.Services;

namespace OliverBooth.Pages.Blog;

/// <summary>
///     Represents the page model for the <c>RawArticle</c> page.
/// </summary>
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

    /// <summary>
    ///     Gets the requested blog post.
    /// </summary>
    /// <value>The requested blog post.</value>
    public BlogPost Post { get; private set; } = null!;

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
