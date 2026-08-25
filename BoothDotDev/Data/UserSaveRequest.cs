namespace BoothDotDev.Data;

/// <summary>
///     Represents a request to create or save a user.
/// </summary>
/// <param name="DisplayName">The display name of the user.</param>
/// <param name="EmailAddress">The email address of the user.</param>
/// <param name="DisableLogin">
///     <see langword="true" /> to disable login for the user, clearing any existing password; otherwise,
///     <see langword="false" />.
/// </param>
/// <param name="NewPassword">
///     The new password for the user, or <see langword="null" /> or whitespace to leave the existing password (if
///     any) unchanged. Ignored entirely when <paramref name="DisableLogin" /> is <see langword="true" />.
/// </param>
/// <param name="TotpSecret">
///     The user's TOTP secret, or <see langword="null" /> or whitespace to clear it and disable TOTP entirely.
/// </param>
public sealed record UserSaveRequest(
    string DisplayName,
    string EmailAddress,
    bool DisableLogin,
    string? NewPassword,
    string? TotpSecret);
