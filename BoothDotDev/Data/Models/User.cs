using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Text;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a user.
/// </summary>
public sealed class User
{
    private const int DefaultAvatarSize = 28;

    /// <summary>
    ///     Gets the URL of the user's avatar.
    /// </summary>
    /// <value>The URL of the user's avatar.</value>
    [NotMapped]
    public Uri AvatarUrl
    {
        get => GetAvatarUrl(DefaultAvatarSize);
    }

    /// <summary>
    ///     Gets or sets the email address of the user.
    /// </summary>
    /// <value>The email address of the user.</value>
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the display name of the author.
    /// </summary>
    /// <value>The display name of the author.</value>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the unique identifier of the user.
    /// </summary>
    /// <value>The unique identifier of the user.</value>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets the date and time the user registered.
    /// </summary>
    /// <value>The registration date and time.</value>
    public DateTimeOffset Registered { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets the TOTP secret for the user.
    /// </summary>
    /// <value>The TOTP secret for the user.</value>
    public string? TotpSecret { get; set; }

    /// <summary>
    ///     Gets or sets the password hash.
    /// </summary>
    /// <value>The password hash.</value>
    internal string Password { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the salt used to hash the password.
    /// </summary>
    /// <value>The salt used to hash the password.</value>
    internal string Salt { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the URL of the author's avatar.
    /// </summary>
    /// <param name="size">The size of the avatar.</param>
    /// <returns>The URL of the author's avatar. 404s if no custom Gravatar is configured, rather than falling back to Gravatar's default silhouette.</returns>
    public Uri GetAvatarUrl(int size)
    {
        if (string.IsNullOrWhiteSpace(EmailAddress))
        {
            return new Uri($"https://www.gravatar.com/avatar/0?size={size}&d=404");
        }

        ReadOnlySpan<char> span = EmailAddress.AsSpan();
        var byteCount = Encoding.UTF8.GetByteCount(span);
        Span<byte> bytes = stackalloc byte[byteCount];
        Encoding.UTF8.GetBytes(span, bytes);

        Span<byte> hash = stackalloc byte[16];
        MD5.TryHashData(bytes, hash, out _);

        using Utf8ValueStringBuilder builder = ZString.CreateUtf8StringBuilder();
        Span<char> hex = stackalloc char[2];
        for (var index = 0; index < hash.Length; index++)
        {
            builder.Append(hash[index].TryFormat(hex, out _, "x2") ? hex : "00");
        }

        return new Uri($"https://www.gravatar.com/avatar/{builder}?size={size}&d=404");
    }
}
