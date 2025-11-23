using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Contact;

internal sealed class Blacklist : PageModel
{
    public IActionResult OnGet()
    {
        return RedirectToPagePermanent("/Contact/Index");
    }
}
