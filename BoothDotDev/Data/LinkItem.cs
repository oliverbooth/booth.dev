using BoothDotDev.Extensions;

namespace BoothDotDev.Data;

/// <summary>
///     Represents a link of a project or creation, ready to show.
/// </summary>
/// <param name="Id">The ID of the link.</param>
/// <param name="Kind">The kind of place the link leads to.</param>
/// <param name="Url">The address the link leads to.</param>
/// <param name="Label">The link's own text, if it has any.</param>
/// <param name="IsPrimary">Whether the link is its owner's primary link.</param>
public sealed record LinkItem(Guid Id, LinkKind Kind, string Url, string? Label, bool IsPrimary)
{
    /// <summary>
    ///     Gets the text to show on the link.
    /// </summary>
    /// <value>The link's label, or the default text for its kind if it has none.</value>
    public string Text
    {
        get => Label ?? Kind.DefaultText;
    }
}
