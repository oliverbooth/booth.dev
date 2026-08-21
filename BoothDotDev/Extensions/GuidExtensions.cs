using DEDrake;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extensions for <see cref="ShortGuid" />.
/// </summary>
public static class GuidExtensions
{
    private const int DefaultCommitShaLength = 7;

    /// <param name="id">The <see cref="Guid" />.</param>
    extension(Guid id)
    {
        /// <summary>
        ///     Gets a commit SHA-like string representation of the <see cref="Guid" />.
        /// </summary>
        /// <param name="length">The length of the commit SHA-like string.</param>
        /// <returns>A commit SHA-like string representation of the <see cref="Guid" />.</returns>
        public string ToCommitSha(int length = DefaultCommitShaLength)
        {
            return length is <= 0 or > 32 ? id.ToString("N") : id.ToString("N")[..length];
        }
    }

    /// <param name="id">The <see cref="ShortGuid" />.</param>
    extension(ShortGuid id)
    {
        /// <summary>
        ///     Gets a commit SHA-like string representation of the <see cref="ShortGuid" />.
        /// </summary>
        /// <param name="length">The length of the commit SHA-like string.</param>
        /// <returns>A commit SHA-like string representation of the <see cref="ShortGuid" />.</returns>
        public string ToCommitSha(int length = DefaultCommitShaLength)
        {
            return length is <= 0 or > 32 ? ((Guid)id).ToString("N") : ((Guid)id).ToString("N")[..length];
        }

        /// <summary>
        ///     Builds a raw URL for a route containing a <see cref="ShortGuid" /> segment, bypassing tag-helper link
        ///     generation.
        /// </summary>
        /// <param name="basePath">The base path of the route.</param>
        /// <returns>A raw URL for the route.</returns>
        /// <remarks>
        ///     This is necessary because <c>RouteOptions.LowercaseUrls</c> lowercases generated URLs
        ///     indiscriminately, which silently corrupts case-sensitive ShortGuid values (base64 is case-sensitive).
        ///     There is no built-in way to exclude a single route/parameter from that global setting.
        /// </remarks>
        public string ToRoute(string basePath)
        {
            return $"{basePath}/{id}";
        }
    }
}
