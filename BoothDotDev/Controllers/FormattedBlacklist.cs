using Microsoft.AspNetCore.Mvc;

namespace BoothDotDev.Controllers;

[Controller]
[Route("contact/blacklist/formatted")]
public class FormattedBlacklistController : Controller
{
    [HttpGet("{format}")]
    public IActionResult OnGet([FromRoute] string format)
    {
        return RedirectToPagePermanent("/Contact/Index");
    }
}
