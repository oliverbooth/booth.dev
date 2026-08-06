using BoothDotDev.Common.Data.Models;
using BoothDotDev.Common.Services;
using Cysharp.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Blog;

/// <summary>
///     Represents the page model for the <c>RawArticle</c> page.
/// </summary>
[Area("blog")]
internal sealed class RawArticle : PageModel
{
    private readonly IBlogPostService _blogPostService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RawArticle" /> class.
    /// </summary>
    /// <param name="blogPostService">The <see cref="IBlogPostService" />.</param>
    public RawArticle(IBlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    public IActionResult OnGet(string slug)
    {
        if (!_blogPostService.TryGetPost(slug, out IBlogPost? post))
        {
            return NotFound();
        }

        Response.Headers.Append("Content-Type", "text/plain; charset=utf-8");

        using Utf8ValueStringBuilder builder = ZString.CreateUtf8StringBuilder();
        builder.AppendLine("# " + post.Title);
        builder.AppendLine($"Author: {post.Author.DisplayName}");

        builder.AppendLine($"Published: {post.Published:R}");
        if (post.Updated.HasValue)
            builder.AppendLine($"Updated: {post.Updated:R}");

        builder.AppendLine();
        builder.AppendLine(post.Body);
        return Content(builder.ToString());
    }
}
