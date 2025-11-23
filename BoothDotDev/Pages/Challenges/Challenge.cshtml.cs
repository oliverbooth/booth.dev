using BoothDotDev.Common.Data.Web;
using BoothDotDev.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Challenges;

internal sealed class Challenge : PageModel
{
    private readonly IDevChallengeService _devChallengeService;

    public Challenge(IDevChallengeService devChallengeService)
    {
        _devChallengeService = devChallengeService;
    }

    public IDevChallenge DevChallenge { get; private set; } = null!;

    public IActionResult OnGet([FromRoute] string id, [FromQuery] string? password = null)
    {
        if (!_devChallengeService.TryGetDevChallenge(id, out var challenge, out bool shouldRedirect))
        {
            return NotFound();
        }

        if (!_devChallengeService.AuthenticateChallenge(challenge.Id, password))
        {
            return Unauthorized();
        }

        if (shouldRedirect)
        {
            return RedirectPermanent(password is not null
                ? $"/challenge/{challenge.Id}?password={password}"
                : $"/challenge/{challenge.Id}");
        }

        DevChallenge = challenge;
        return Page();
    }
}
