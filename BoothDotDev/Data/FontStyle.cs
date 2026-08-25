using NpgsqlTypes;

namespace BoothDotDev.Data;

/// <summary>
///     Represents an enumeration of font styles for text rendering.
/// </summary>
public enum FontStyle
{
    /// <summary>
    ///     Sans-serif font style.
    /// </summary>
    [PgName("sans_serif")] SansSerif,

    /// <summary>
    ///     Serif font style.
    /// </summary>
    [PgName("serif")] Serif,
}
