namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a music item.
/// </summary>
public sealed class MusicItem : CreativeItem
{
    /// <summary>
    ///     Gets or sets the duration of the track.
    /// </summary>
    /// <value>A <see cref="TimeSpan" /> representing the duration of the track.</value>
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;
}
