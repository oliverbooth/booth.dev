namespace BoothDotDev.Data;

/// <summary>
///     Represents a project or creation as the portfolio admin page lists it.
/// </summary>
/// <param name="ItemId">The ID of the project or creation.</param>
/// <param name="EntryId">The ID of its portfolio entry, or <see langword="null" /> if it is not listed.</param>
/// <param name="Title">The name of the project or title of the creation.</param>
/// <param name="Kind">The kind of work it is.</param>
/// <param name="Hue">The colour it is shown in.</param>
/// <param name="Label">The name of the kind of work, such as <c>code</c> or <c>3d</c>.</param>
/// <param name="IsFeatured">Whether it is featured on the home page.</param>
/// <param name="HiddenReason">
///     Why it is not shown on the public portfolio even though it is listed, or <see langword="null" /> if it is shown.
/// </param>
public sealed record PortfolioAdminItem(
    Guid ItemId,
    Guid? EntryId,
    string Title,
    PortfolioItemKind Kind,
    PaletteHue Hue,
    string Label,
    bool IsFeatured,
    string? HiddenReason);
