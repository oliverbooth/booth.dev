namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a WebAuthn passkey credential registered by a user.
/// </summary>
public sealed class PasskeyCredential
{
    /// <summary>
    ///     Gets the authenticator attestation GUID, identifying the make/model of the authenticator this
    ///     credential was created on.
    /// </summary>
    /// <value>The authenticator attestation GUID.</value>
    public Guid AaGuid { get; internal set; }

    /// <summary>
    ///     Gets the WebAuthn credential ID.
    /// </summary>
    /// <value>The WebAuthn credential ID.</value>
    public byte[] CredentialId { get; internal set; } = [];

    /// <summary>
    ///     Gets the date and time this credential was registered.
    /// </summary>
    /// <value>The date and time this credential was registered.</value>
    public DateTimeOffset CreatedAt { get; internal set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets the unique identifier of the credential.
    /// </summary>
    /// <value>The unique identifier of the credential.</value>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets or sets the date and time this credential was last used to sign in.
    /// </summary>
    /// <value>The date and time this credential was last used to sign in, or <see langword="null" /> if never.</value>
    public DateTimeOffset? LastUsedAt { get; set; }

    /// <summary>
    ///     Gets or sets a user-supplied label for this credential, e.g. "MacBook Touch ID".
    /// </summary>
    /// <value>The credential's nickname.</value>
    public string? Nickname { get; set; }

    /// <summary>
    ///     Gets the COSE-encoded public key for this credential.
    /// </summary>
    /// <value>The public key.</value>
    public byte[] PublicKey { get; internal set; } = [];

    /// <summary>
    ///     Gets or sets the signature counter, incremented by the authenticator on every use as a replay-protection
    ///     measure.
    /// </summary>
    /// <value>The signature counter.</value>
    public long SignatureCounter { get; set; }

    /// <summary>
    ///     Gets or sets a comma-separated list of transports this credential is available over (e.g.
    ///     <c>usb,nfc</c>), used only to hint which icon to show - never security-relevant.
    /// </summary>
    /// <value>The credential's transports.</value>
    public string? Transports { get; set; }

    /// <summary>
    ///     Gets the ID of the user this credential belongs to.
    /// </summary>
    /// <value>The ID of the owning user.</value>
    public Guid UserId { get; internal set; }
}
