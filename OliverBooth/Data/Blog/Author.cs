using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace OliverBooth.Data.Blog;

/// <summary>
///     Represents an author of a blog post.
/// </summary>
public sealed class Author : IEquatable<Author>
{
    [NotMapped]
    public string AvatarHash
    {
        get
        {
            if (EmailAddress is null)
            {
                return string.Empty;
            }

            using var md5 = MD5.Create();
            ReadOnlySpan<char> span = EmailAddress.AsSpan();
            int byteCount = Encoding.UTF8.GetByteCount(span);
            Span<byte> bytes = stackalloc byte[byteCount];
            Encoding.UTF8.GetBytes(span, bytes);

            Span<byte> hash = stackalloc byte[16];
            md5.TryComputeHash(bytes, hash, out _);

            var builder = new StringBuilder();
            foreach (byte b in hash)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }
    }

    /// <summary>
    ///     Gets or sets the email address of the author.
    /// </summary>
    /// <value>The email address.</value>
    public string? EmailAddress { get; set; }

    /// <summary>
    ///     Gets the ID of the author.
    /// </summary>
    /// <value>The ID.</value>
    public int Id { get; private set; }

    /// <summary>
    ///     Gets or sets the name of the author.
    /// </summary>
    /// <value>The name.</value>
    public string Name { get; set; } = string.Empty;

    public bool Equals(Author? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Author other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id;
    }
}
