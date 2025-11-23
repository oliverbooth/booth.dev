using Microsoft.AspNetCore.Mvc;

namespace BoothDotDev.Controllers;

/// <summary>
///     Represents the formatted blacklist controller.
/// </summary>
[Controller]
[Route("contact/blacklist/formatted")]
public sealed class FormattedBlacklistController : Controller
{
    /// <summary>
    ///     Handles the incoming GET request to the controller.
    /// </summary>
    /// <param name="format">The requested format.</param>
    /// <returns>>A redirection to the contact page.</returns>
    [HttpGet("{format}")]
    public IActionResult OnGet([FromRoute] string format)
    {
        return RedirectToPagePermanent("/Contact/Index");
    }
}
