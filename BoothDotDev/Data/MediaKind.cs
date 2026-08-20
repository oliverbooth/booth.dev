namespace BoothDotDev.Data;

/// <summary>
///     Represents the kind of media a resolved CDN URL points to, used to select the correct rendering partial.
/// </summary>
public enum MediaKind
{
    /// <summary>
    ///     The media is an image (png, jpg, jpeg, gif, webp, svg).
    /// </summary>
    Image,

    /// <summary>
    ///     The media is a video (mp4, webm, mov).
    /// </summary>
    Video,

    /// <summary>
    ///     The media is an audio file (mp3, wav, ogg, flac).
    /// </summary>
    Audio,

    /// <summary>
    ///     The media is of an unrecognized or unsupported type.
    /// </summary>
    Misc
}
