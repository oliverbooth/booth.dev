using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OliverBooth.Pages.Contact;

public class Privacy : PageModel
{
    public string Which { get; private set; } = "website";

    public void OnGet(string? which = "website")
    {
        Which = which ?? "website";
    }

    public void SubmitForm()
    {
    }
}
