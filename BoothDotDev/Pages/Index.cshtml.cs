using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages;

public class Index : PageModel
{
    public IActionResult OnGet()
    {
        return RedirectToPage("/Blog/Index");
    }
}
