using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Blog;

/// <summary>
///     Represents a class which defines the model for the <c>/blog/page/#</c> route.
/// </summary>
public class List : PageModel
{
    /// <summary>
    ///     Gets the requested page number.
    /// </summary>
    /// <value>The requested page number.</value>
    public int PageNumber { get; private set; }

    public string[] Tag { get; private set; } = [];

    /// <summary>
    ///     Handles the incoming GET request to the page.
    /// </summary>
    /// <param name="page">The requested page number, starting from 1.</param>
    /// <param name="tag">The tag by which to filter results.</param>
    /// <returns></returns>
    public IActionResult OnGet([FromRoute(Name = "pageNumber")] int page = 1, [FromQuery(Name = "tag")] string? tag = null)
    {
        if (page < 2)
        {
            return RedirectToPage("Index");
        }

        PageNumber = page;
        Tag = tag?.Split('+') ?? [];
        return Page();
    }
}
