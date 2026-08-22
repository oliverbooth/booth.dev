namespace BoothDotDev.Data;

/// <summary>
///     Represents the options for WebAuthn (passkey) authentication.
/// </summary>
public sealed class WebAuthnOptions
{
    /// <summary>
    ///     The name of the configuration section for WebAuthn options.
    /// </summary>
    public const string SectionName = "WebAuthn";

    /// <summary>
    ///     Gets or sets the relying party ID.
    /// </summary>
    /// <value>The relying party ID.</value>
    /// <remarks>
    ///     The relying party ID is the bare domain that a passkey is bound to, with no scheme or port (e.g. <c>booth.dev</c>).
    ///     It must never change once passkeys have been registered against it, as doing so invalidates every existing passkey.
    /// </remarks>
    public string RpId { get; init; } = string.Empty;

    /// <summary>
    ///     Gets or sets the set of origins (scheme + host + port) from which a WebAuthn ceremony is allowed to be performed.
    /// </summary>
    /// <value>The allowed origins.</value>
    public List<string> Origins { get; init; } = [];
}
