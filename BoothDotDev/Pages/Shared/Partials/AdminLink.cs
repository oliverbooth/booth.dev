namespace BoothDotDev.Pages.Shared.Partials;

/// <summary>
///     Represents a link to a page of the admin area, shown to a signed-in admin on the public page it relates to.
/// </summary>
/// <param name="Text">The text of the link.</param>
/// <param name="Page">The path of the Razor page the link leads to.</param>
/// <param name="RouteValues">The route values for the page, if it needs any.</param>
public sealed record AdminLink(string Text, string Page, object? RouteValues = null);
