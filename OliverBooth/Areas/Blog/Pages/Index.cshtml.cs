using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OliverBooth.Data.Blog;
using OliverBooth.Services;

namespace OliverBooth.Areas.Blog.Pages;

[Area("blog")]
public class Index : PageModel
{
    private readonly BlogService _blogService;

    public Index(BlogService blogService)
    {
        _blogService = blogService;
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
