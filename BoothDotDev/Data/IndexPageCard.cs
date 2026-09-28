namespace BoothDotDev.Data;

/// <summary>
///     Represents the bespoke Open Graph card and Discord embed for a listing page that has no content of its own to
///     hang one off, such as an index or category page.
/// </summary>
/// <param name="Badge">The all-caps badge shown above the title, such as <c>CHALLENGES</c>.</param>
/// <param name="Title">The page's title.</param>
/// <param name="Description">The description shown under the title.</param>
/// <param name="Hue">The hue the card is tinted with.</param>
/// <param name="ImageKey">The card's cache key, served at <c>/og/{ImageKey}.png</c>.</param>
public sealed record IndexPageCard(string Badge, string Title, string Description, PaletteHue Hue, string ImageKey);
