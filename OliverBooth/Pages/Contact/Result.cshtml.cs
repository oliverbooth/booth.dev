using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OliverBooth.Pages.Contact;

public class Result : PageModel
{
    public bool WasSuccessful { get; private set; }

    public IActionResult OnGet()
    {
        if (!TempData.ContainsKey("Success"))
        {
            return RedirectToPage("/Contact/Index");
        }

#pragma warning disable S1125
        WasSuccessful = TempData["Success"] is true;
#pragma warning restore S1125
        TempData.Remove("Success");
        return Page();
    }
}
