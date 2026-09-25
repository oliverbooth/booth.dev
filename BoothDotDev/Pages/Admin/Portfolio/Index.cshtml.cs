using BoothDotDev.Data;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Portfolio;

/// <summary>
///     Represents the page model for the admin portfolio page, where the portfolio's order and the home page's featured
///     items are set.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Index : PageModel
{
    private readonly PortfolioService _portfolioService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="portfolioService">The <see cref="PortfolioService" />.</param>
    public Index(PortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    /// <summary>
    ///     Gets everything the page shows.
    /// </summary>
    /// <value>The <see cref="PortfolioAdminView" />.</value>
    public PortfolioAdminView View { get; private set; } = new([], [], []);

    /// <summary>
    ///     Gets the number of featured items the home page shows.
    /// </summary>
    /// <value>The featured limit.</value>
    public int FeaturedLimit
    {
        get => PortfolioService.FeaturedLimit;
    }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        View = _portfolioService.GetAdminView();
    }

    /// <summary>
    ///     Handles the POST request for saving the portfolio's order.
    /// </summary>
    /// <param name="ids">The entry IDs, in their new order.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostReorder(List<Guid> ids)
    {
        _portfolioService.Reorder(ids);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for saving the order of the featured items.
    /// </summary>
    /// <param name="ids">The entry IDs, in their new order.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostReorderFeatured(List<Guid> ids)
    {
        _portfolioService.ReorderFeatured(ids);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for listing a project or creation on the portfolio.
    /// </summary>
    /// <param name="id">The ID of the project or creation.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostList(Guid id)
    {
        _portfolioService.List(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for removing an entry from the portfolio.
    /// </summary>
    /// <param name="id">The ID of the entry.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostUnlist(Guid id)
    {
        _portfolioService.Unlist(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for featuring an entry on the home page.
    /// </summary>
    /// <param name="id">The ID of the entry.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostFeature(Guid id)
    {
        _portfolioService.Feature(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for removing an entry from the home page's featured items.
    /// </summary>
    /// <param name="id">The ID of the entry.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostUnfeature(Guid id)
    {
        _portfolioService.Unfeature(id);
        return RedirectToPage();
    }
}
