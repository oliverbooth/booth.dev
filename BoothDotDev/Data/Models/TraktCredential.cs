namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents the stored Trakt OAuth token pair. There is only ever one row - this app talks to Trakt as a
///     single user, not on behalf of visitors.
/// </summary>
public sealed class TraktCredential
{
    /// <summary>
    ///     The fixed ID of the single credential row.
    /// </summary>
    public const int SingletonId = 1;

    /// <summary>
    ///     Gets or sets the ID of the row. Always <see cref="SingletonId" />.
    /// </summary>
    /// <value>The ID of the row.</value>
    public int Id { get; set; } = SingletonId;

    /// <summary>
    ///     Gets or sets the current access token.
    /// </summary>
    /// <value>The access token.</value>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the current refresh token. Refresh tokens are single-use - this is replaced every time the
    ///     access token is refreshed.
    /// </summary>
    /// <value>The refresh token.</value>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the date and time at which the access token expires.
    /// </summary>
    /// <value>The expiry date and time of the access token.</value>
    public DateTimeOffset ExpiresAt { get; set; }
}
