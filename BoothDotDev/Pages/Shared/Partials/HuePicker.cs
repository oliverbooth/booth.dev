using BoothDotDev.Data;

namespace BoothDotDev.Pages.Shared.Partials;

/// <summary>
///     Represents the data shown by the admin hue picker: a set of swatches for choosing a <see cref="PaletteHue" />,
///     plus an "auto" swatch that clears the choice.
/// </summary>
public sealed class HuePicker
{
    /// <summary>
    ///     Gets the name of the form field the picker posts, e.g. <c>Input.Color</c>.
    /// </summary>
    /// <value>The form field name.</value>
    public required string Name { get; init; }

    /// <summary>
    ///     Gets the currently-selected hue.
    /// </summary>
    /// <value>The selected hue, or <see langword="null" /> if the "auto" swatch is selected.</value>
    public PaletteHue? Selected { get; init; }

    /// <summary>
    ///     Gets the text describing what "auto" resolves to for this kind of item, shown as the swatch's tooltip.
    /// </summary>
    /// <value>The description of the fallback, e.g. "derived from its position in the list".</value>
    public required string AutoDescription { get; init; }
}
