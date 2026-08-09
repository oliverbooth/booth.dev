using DEDrake;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extensions for <see cref="ShortGuid"/>.
/// </summary>
public static class ShortGuidExtensions
{
    /// <param name="id">The <see cref="ShortGuid"/> to convert.</param>
    extension(ShortGuid id)
    {
        /// <summary>
        ///     Builds a raw URL for a route containing a <see cref="ShortGuid"/> segment, bypassing tag-helper link
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
