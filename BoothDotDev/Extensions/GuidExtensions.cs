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
            return length is <= 0 or > 32 ? id.ToString("N") : id.ToString("N")[^length..];
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
            return length is <= 0 or > 32 ? ((Guid)id).ToString("N") : ((Guid)id).ToString("N")[^length..];
        }
    }
}
