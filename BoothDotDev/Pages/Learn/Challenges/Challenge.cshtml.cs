using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Learn.Challenges;

internal sealed class Challenge : PageModel
{
    private readonly DevChallengeService _devChallengeService;

    public Challenge(DevChallengeService devChallengeService)
    {
        _devChallengeService = devChallengeService;
    }

    public DevChallenge DevChallenge { get; private set; } = null!;

    public IActionResult OnGet([FromRoute] string id, [FromQuery] string? password = null)
    {
        if (!_devChallengeService.TryGetDevChallenge(id, out var challenge, out var shouldRedirect))
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
