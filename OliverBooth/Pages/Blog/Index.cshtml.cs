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

    public IActionResult OnGet([FromQuery(Name = "p")] int? postId = null)
    {
        if (!postId.HasValue) return Page();
        if (!_blogService.TryGetWordPressBlogPost(postId.Value, out BlogPost? post)) return NotFound();

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
