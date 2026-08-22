using BoothDotDev.Services;

namespace BoothDotDev.Data;

/// <summary>
///     Represents the contents of a single directory under the CDN mount.
/// </summary>
/// <param name="Entries">The files and folders directly inside the directory, folders first, then alphabetical.</param>
/// <param name="ResolvedPath">The resolved path of the listed directory.</param>
public sealed record CdnDirectoryListing(IReadOnlyList<CdnEntry> Entries, CdnPath ResolvedPath);

/// <summary>
///     Represents a bounded preview of how many items a recursive delete would remove.
/// </summary>
/// <param name="ItemCount">The number of items found, capped at the preview's item cap.</param>
/// <param name="Capped"><see langword="true" /> if the real item count may be higher than <paramref name="ItemCount" />.</param>
public sealed record CdnDeletePreview(int ItemCount, bool Capped);
