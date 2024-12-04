using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OliverBooth.Pages;

public class Index : PageModel
{
    public IActionResult OnGet()
    {
        return RedirectToPage("/Blog/Index");
    }
}
