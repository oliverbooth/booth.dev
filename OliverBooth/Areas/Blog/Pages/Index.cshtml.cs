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
            area = "blog",
            year = post.Published.ToString("yyyy"),
            month = post.Published.ToString("MM"),
            day = post.Published.ToString("dd"),
            slug = post.Slug
        };
        return Redirect(Url.Page("/Article", route)!);
    }
}
