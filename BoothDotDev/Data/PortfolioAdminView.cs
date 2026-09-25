namespace BoothDotDev.Data;

/// <summary>
///     Represents everything the portfolio admin page shows.
/// </summary>
/// <param name="Listed">Every listed item, in portfolio order.</param>
/// <param name="Featured">Every featured item, in featured order.</param>
/// <param name="Unlisted">Every project and creation that is not listed.</param>
public sealed record PortfolioAdminView(
    IReadOnlyList<PortfolioAdminItem> Listed,
    IReadOnlyList<PortfolioAdminItem> Featured,
    IReadOnlyList<PortfolioAdminItem> Unlisted);
