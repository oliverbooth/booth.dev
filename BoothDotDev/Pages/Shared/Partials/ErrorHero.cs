namespace BoothDotDev.Pages.Shared.Partials;

/// <summary>
///     Represents the data shown on an error page: an icon tile, the status code, a title, a description, and the
///     available actions.
/// </summary>
public sealed class ErrorHero
{
    /// <summary>
    ///     Gets the HTTP status code.
    /// </summary>
    /// <value>The status code.</value>
    public required int Code { get; init; }

    /// <summary>
    ///     Gets the title shown under the icon.
    /// </summary>
    /// <value>The title.</value>
    public required string Title { get; init; }

    /// <summary>
    ///     Gets the description shown under the title.
    /// </summary>
    /// <value>The description.</value>
    public required string Body { get; init; }

    /// <summary>
    ///     Gets the short label shown at the bottom of the card, such as <c>not found</c>.
    /// </summary>
    /// <value>The tag text.</value>
    public required string Tag { get; init; }

    /// <summary>
    ///     Gets the Tabler icon class shown in the icon tile, such as <c>ti-lock</c>.
    /// </summary>
    /// <value>The icon class.</value>
    public required string Icon { get; init; }

    /// <summary>
    ///     Gets the CSS modifier selecting the icon tile's colour, such as <c>danger</c>.
    /// </summary>
    /// <value>The tint modifier. The default is <c>neutral</c>.</value>
    public string Tint { get; init; } = "neutral";

    /// <summary>
    ///     Gets a value indicating whether a "try again" button is shown alongside the home link.
    /// </summary>
    /// <value><see langword="true" /> to show the retry button; otherwise, <see langword="false" />.</value>
    public bool ShowRetry { get; init; }
}
