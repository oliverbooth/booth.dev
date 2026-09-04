using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace BoothDotDev.Services;

/// <summary>
///     Defines the upload safety rails shared by every service that accepts files onto the CDN mount: which extensions are
///     allowed, how large a single upload may be, and how raster images are sanitized before being written to disk.
/// </summary>
public static class CdnUploadPolicy
{
    /// <summary>
    ///     The maximum size, in bytes, of a single upload.
    /// </summary>
    public const long MaxUploadSizeBytes = 500L * 1024 * 1024;

    /// <summary>
    ///     The file extensions accepted for upload.
    /// </summary>
    public static readonly HashSet<string> AllowedExtensions =
    [
        with(StringComparer.OrdinalIgnoreCase),
        "png", "jpg", "jpeg", "gif", "webp", "svg",
        "mp4", "webm", "mov",
        "mp3", "wav", "ogg", "flac",
        "pdf", "zip", "txt"
    ];

    /// <summary>
    ///     The raster image extensions that get EXIF/IPTC/XMP metadata stripped on upload.
    /// </summary>
    public static readonly HashSet<string> StrippableImageExtensions =
        [with(StringComparer.OrdinalIgnoreCase), "png", "jpg", "jpeg", "webp"];

    /// <summary>
    ///     Decodes an image, bakes in its EXIF orientation, strips all EXIF/IPTC/XMP metadata, and re-encodes it.
    /// </summary>
    /// <param name="source">A seekable stream containing the raw uploaded image bytes.</param>
    /// <param name="destination">The stream to write the sanitized image to.</param>
    /// <exception cref="UnknownImageFormatException"><paramref name="source" /> isn't a decodable image.</exception>
    public static void StripImageMetadata(Stream source, Stream destination)
    {
        using var image = Image.Load(source);
        var format = image.Metadata.DecodedImageFormat
                     ?? throw new UnknownImageFormatException("Could not determine the decoded image's format.");

        // auto-orient the image based on EXIF orientation, lest stripping the EXIF block leave it incorrectly rotated
        image.Mutate(x => x.AutoOrient());

        image.Metadata.ExifProfile = null;
        image.Metadata.IptcProfile = null;
        image.Metadata.XmpProfile = null;
        // ICC color profile is deliberately kept

        var encoder = format.Name switch
        {
            "JPEG" => new JpegEncoder { Quality = 100 },
            "WEBP" => new WebpEncoder { Quality = 100 },
            _ => image.Configuration.ImageFormatsManager.GetEncoder(format)
                 ?? throw new NotSupportedException($"No encoder is registered for image format '{format.Name}'.")
        };

        image.Save(destination, encoder);
    }
}
