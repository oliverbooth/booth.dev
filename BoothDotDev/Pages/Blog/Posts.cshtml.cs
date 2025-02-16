using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Blog;

public class Posts : PageModel
{
    /// <summary>
    ///     Gets the requested page number.
    /// </summary>
    /// <value>The requested page number.</value>
    public int PageNumber { get; private set; }

    public IActionResult OnGet([FromRoute(Name = "pageNumber")] int page = 1)
    {
        if (page < 1)
        {
            return RedirectToPage("Posts");
        }

        PageNumber = page;
        return Page();
    }
}
