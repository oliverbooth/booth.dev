using BoothDotDev.Data;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Portfolio;

/// <summary>
///     Represents the page model for the "stuff i made" page - a merged view of Projects and Create.
/// </summary>
public sealed class Made : PageModel
{
    private readonly PortfolioService _portfolioService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Made" /> class.
    /// </summary>
    /// <param name="portfolioService">The portfolio service.</param>
    public Made(PortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    /// <summary>
    ///     Gets everything listed on the portfolio, in the order set in the admin.
    /// </summary>
    /// <value>The items.</value>
    public IReadOnlyList<PortfolioItem> Items { get; private set; } = [];

    /// <summary>
    ///     Handles the HTTP GET request.
    /// </summary>
    public void OnGet()
    {
        Items = _portfolioService.GetAll();
    }
}
