using System.Text.RegularExpressions;
using FluentResults;

namespace BoothDotDev.Services;

/// <summary>
///     Validates individual file and folder names under the CDN mount, shared by every service that names files there.
/// </summary>
public static partial class CdnNaming
{
    /// <summary>
    ///     Validates a single path segment - a bare file or folder name, never a multi-segment path.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <returns>A result containing the validated name, or a failure describing why it was rejected.</returns>
    public static Result<string> ValidateSegment(string? name)
    {
        var value = name ?? string.Empty;

        if (!SafeSegmentPattern().IsMatch(value))
        {
            return Result.Fail("Names may only contain letters, numbers, dots, dashes, and underscores.");
        }

        return value;
    }

    [GeneratedRegex(@"^[a-zA-Z0-9][a-zA-Z0-9._-]{0,119}$")]
    private static partial Regex SafeSegmentPattern();
}
