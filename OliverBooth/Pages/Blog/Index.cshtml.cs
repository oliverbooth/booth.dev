using System.Diagnostics;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OliverBooth.Data.Blog;
using OliverBooth.Services;

namespace OliverBooth.Pages.Blog;

public class Index : PageModel
{
    private readonly BlogService _blogService;

    public Index(BlogService blogService)
    {
        _blogService = blogService;
    }

    public string SanitizeContent(string content)
    {
        content = content.Replace("<!--more-->", string.Empty);

        while (content.Contains("\n\n"))
        {
            content = content.Replace("\n\n", "\n");
        }

        return Markdig.Markdown.ToHtml(content.Trim());
    }

    public string TrimContent(string content, out bool trimmed)
    {
        ReadOnlySpan<char> span = content.AsSpan();
        int moreIndex = span.IndexOf("<!--more-->", StringComparison.Ordinal);
        trimmed = moreIndex != -1 || span.Length > 256;
        return moreIndex != -1 ? span[..moreIndex].Trim().ToString() : content.Truncate(256);
    }

    public IActionResult OnGet([FromQuery(Name = "pid")] int? postId = null,
        [FromQuery(Name = "p")] int? wpPostId = null)
    {
        if (postId.HasValue == wpPostId.HasValue)
        {
            return Page();
        }

        return postId.HasValue ? HandleNewRoute(postId.Value) : HandleWordPressRoute(wpPostId!.Value);
    }

    private IActionResult HandleNewRoute(int postId)
    {
        return _blogService.TryGetBlogPost(postId, out BlogPost? post) ? RedirectToPost(post) : NotFound();
    }

    private IActionResult HandleWordPressRoute(int wpPostId)
    {
        return _blogService.TryGetWordPressBlogPost(wpPostId, out BlogPost? post) ? RedirectToPost(post) : NotFound();
    }

    private IActionResult RedirectToPost(BlogPost post)
    {
        var route = new
        {
            year = post.Published.Year,
            month = post.Published.Month,
            day = post.Published.Day,
            slug = post.Slug
        };
        return Redirect(Url.Page("/Blog/Article", route)!);
    }
}
