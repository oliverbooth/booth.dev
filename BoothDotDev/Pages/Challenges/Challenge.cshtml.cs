using BoothDotDev.Common.Data.Web;
using BoothDotDev.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Challenges;

public class Challenge : PageModel
{
    private readonly IDevChallengeService _devChallengeService;

    public Challenge(IDevChallengeService devChallengeService)
    {
        _devChallengeService = devChallengeService;
    }

    public IDevChallenge DevChallenge { get; private set; } = null!;

    public IActionResult OnGet([FromQuery] string? password = null)
    {
        var challenge = _devChallengeService.GetDevChallenges().FirstOrDefault();
        if (challenge is null)
        {
            return NotFound();
        }

        if (!_devChallengeService.AuthenticateChallenge(challenge.Id, password))
        {
            return Unauthorized();
        }

        DevChallenge = challenge;
        return Page();
    }
}
