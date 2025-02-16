using BoothDotDev.Common.Data.Web;
using NetBarcode;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace BoothDotDev.Data.Web;

/// <summary>
///     Represents a book.
/// </summary>
internal sealed class Book : IBook
{
    /// <inheritdoc />
    public string Author { get; }

    /// <inheritdoc />
    public string Isbn { get; }

    /// <inheritdoc />
    public BookState State { get; }

    /// <inheritdoc />
    public string Title { get; }

    public string GetBarcode()
    {
        var barcode = new Barcode(Isbn, NetBarcode.Type.EAN13);
        using var image = barcode.GetImage();
        int width = image.Width;
        int height = image.Height;
        image.Mutate(i => i.Pad(width + 10, height + 10, Color.White));
        image.Mutate(i => i.Resize(i.GetCurrentSize() / 4 * 3));

        return image.ToBase64String(PngFormat.Instance);
    }
}