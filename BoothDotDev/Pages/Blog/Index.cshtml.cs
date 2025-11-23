using BoothDotDev.Common.Data.Blog;
using BoothDotDev.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Blog;

[Area("blog")]
internal sealed class Index : PageModel
{
    private readonly IBlogPostService _blogPostService;

    public Index(IBlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    public string[] Tag { get; private set; } = [];

    public IActionResult OnGet([FromQuery(Name = "pid")] Guid? postId = null,
        [FromQuery(Name = "p")] int? wpPostId = null,
        [FromQuery(Name = "tag")] string? tag = null)
    {
        ViewData["Tags"] = Tag = tag?.Split('+') ?? [];

        if (postId.HasValue == wpPostId.HasValue)
        {
            return Page();
        }

        return postId.HasValue ? HandleNewRoute(postId.Value) : HandleWordPressRoute(wpPostId!.Value);
    }

    private IActionResult HandleNewRoute(Guid postId)
    {
        return _blogPostService.TryGetPost(postId, out IBlogPost? post) ? RedirectToPost(post) : NotFound();
    }

    private IActionResult HandleWordPressRoute(int wpPostId)
    {
        return _blogPostService.TryGetPost(wpPostId, out IBlogPost? post) ? RedirectToPost(post) : NotFound();
    }

    private RedirectResult RedirectToPost(IBlogPost post)
    {
        var route = new
        {
            year = post.Published.ToString("yyyy"),
            month = post.Published.ToString("MM"),
            day = post.Published.ToString("dd"),
            slug = post.Slug
        };
        return Redirect(Url.Page("/Blog/Article", route)!);
    }
}
