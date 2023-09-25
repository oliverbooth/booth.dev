using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OliverBooth.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public int HttpStatusCode { get; private set; }

    public IActionResult OnGet(int? code = null)
    {
        HttpStatusCode = code ?? HttpContext.Response.StatusCode;
        if (HttpStatusCode == 200)
        {
            return RedirectToPage("/Index");
        }

        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return Page();
    }
}
