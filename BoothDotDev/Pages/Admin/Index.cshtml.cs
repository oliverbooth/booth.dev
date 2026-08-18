using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin;

[Authorize("Admin")]
public class Index : PageModel
{
    public void OnGet()
    {
    }
}
