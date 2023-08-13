using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OliverBooth.Data.Blog;
using OliverBooth.Services;

namespace OliverBooth.Pages.Blog;

[Area("blog")]
public class Index : PageModel
{
    private readonly IBlogPostService _blogPostService;

    public Index(IBlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    public IActionResult OnGet([FromQuery(Name = "pid")] Guid? postId = null,
        [FromQuery(Name = "p")] int? wpPostId = null)
    {
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

    private IActionResult RedirectToPost(IBlogPost post)
    {
        var route = new
        {
            area = "blog",
            year = post.Published.ToString("yyyy"),
            month = post.Published.ToString("MM"),
            day = post.Published.ToString("dd"),
            slug = post.Slug
        };
        return Redirect(Url.Page("/Article", route)!);
    }
}
