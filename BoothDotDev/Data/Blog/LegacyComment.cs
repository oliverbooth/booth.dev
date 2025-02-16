using System.Web;
using BoothDotDev.Common.Data.Blog;

namespace BoothDotDev.Data.Blog;

internal sealed class LegacyComment : ILegacyComment
{
    /// <inheritdoc />
    public string? Avatar { get; private set; }

    /// <inheritdoc />
    public string Author { get; private set; } = string.Empty;

    /// <inheritdoc />
    public string Body { get; private set; } = string.Empty;

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; private set; }

    /// <inheritdoc />
    public Guid Id { get; private set; }

    /// <inheritdoc />
    public Guid? ParentComment { get; private set; }

    /// <inheritdoc />
    public Guid PostId { get; private set; }

    /// <inheritdoc />
    public string GetAvatarUrl()
    {
        return Avatar ?? $"https://ui-avatars.com/api/?name={HttpUtility.UrlEncode(Author)}";
    }
}
