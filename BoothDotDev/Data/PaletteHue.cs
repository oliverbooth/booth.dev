using NpgsqlTypes;

namespace BoothDotDev.Data;

/// <summary>
///     Represents one of the site's seven accent hues, used to color-code content by type, category, or folder.
/// </summary>
public enum PaletteHue
{
    /// <summary>
    ///     The site's primary brand hue (blurple).
    /// </summary>
    [PgName("brand")] Brand,

    /// <summary>
    ///     Grape.
    /// </summary>
    [PgName("grape")] Grape,

    /// <summary>
    ///     Bubblegum pink.
    /// </summary>
    [PgName("pink")] Pink,

    /// <summary>
    ///     Tangerine.
    /// </summary>
    [PgName("tangerine")] Tangerine,

    /// <summary>
    ///     Sunshine.
    /// </summary>
    [PgName("sun")] Sun,

    /// <summary>
    ///     Mint.
    /// </summary>
    [PgName("mint")] Mint,

    /// <summary>
    ///     Sky.
    /// </summary>
    [PgName("sky")] Sky
}
