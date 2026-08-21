using System.Drawing;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents an artwork item.
/// </summary>
public sealed class ArtworkItem : CreativeItem
{
    /// <summary>
    ///     Gets or sets the duration of the track.
    /// </summary>
    /// <value>A <see cref="TimeSpan" /> representing the duration of the track.</value>
    public Size Resolution { get; set; } = default;
}
