using FluentResults;

namespace BoothDotDev.Services;

/// <summary>
///     Resolves paths under the CDN mount, shared by every service that reads or writes files there.
/// </summary>
public static class CdnPaths
{
    /// <summary>
    ///     Gets the physical root directory of the CDN mount.
    /// </summary>
    /// <returns>The CDN root directory.</returns>
    public static string GetRoot()
    {
        return Path.Combine(AppContext.BaseDirectory, "cdn");
    }

    /// <summary>
    ///     Resolves a user-supplied relative path to an absolute path confined to <paramref name="root" />, rejecting any
    ///     input that would otherwise escape it.
    /// </summary>
    /// <param name="root">The CDN root directory to confine the resolved path to.</param>
    /// <param name="relativePath">
    ///     The relative path to resolve, e.g. <c>foo/bar</c> or <c>/foo/bar</c>. A <see langword="null" /> or empty value
    ///     resolves to <paramref name="root" /> itself.
    /// </param>
    /// <returns>A result containing the resolved path, or a failure if <paramref name="relativePath" /> is invalid.</returns>
    /// <remarks>
    ///     This is purely lexical - it never touches the filesystem, and it doesn't resolve symlinks. A symlink planted
    ///     inside <paramref name="root" /> that points outside it would not be caught here.
    /// </remarks>
    public static Result<CdnPath> ResolveRelative(string root, string? relativePath)
    {
        var input = relativePath ?? string.Empty;
        if (input.Contains('\\'))
        {
            return Result.Fail("Invalid path.");
        }

        var segments = input.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            if (segment is "." or "..")
            {
                return Result.Fail("Invalid path.");
            }

            var validated = CdnNaming.ValidateSegment(segment);
            if (validated.IsFailed)
            {
                return validated.ToResult<CdnPath>();
            }
        }

        var normalizedRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        var candidate = segments.Length == 0
            ? normalizedRoot
            : Path.GetFullPath(Path.Combine([normalizedRoot, .. segments]));

        // belt-and-suspenders re-check, even though the segment-level guards above should already guarantee this
        if (candidate != normalizedRoot &&
            !candidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            return Result.Fail("Invalid path.");
        }

        return new CdnPath(candidate, "/" + string.Join('/', segments), segments);
    }
}

/// <summary>
///     Represents a relative path under the CDN root, resolved to an absolute filesystem path.
/// </summary>
/// <param name="AbsolutePath">The resolved absolute filesystem path.</param>
/// <param name="DisplayPath">The normalized, root-relative display path (e.g. <c>/foo/bar</c>).</param>
/// <param name="Segments">The individual path segments.</param>
public sealed record CdnPath(string AbsolutePath, string DisplayPath, IReadOnlyList<string> Segments);
