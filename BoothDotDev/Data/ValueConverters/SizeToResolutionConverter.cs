using System.Drawing;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BoothDotDev.Data.ValueConverters;

internal sealed class SizeToResolutionConverter() : ValueConverter<Size, string>(v => ToResolution(v), v => FromResolution(v))
{
    private static string ToResolution(Size size)
    {
        return $"{size.Width}x{size.Height}";
    }

    private static Size FromResolution(string resolution)
    {
        var index = resolution.IndexOf('x');
        var width = int.Parse(resolution[..index]);
        var height = int.Parse(resolution[(index + 1)..]);
        return new Size(width, height);
    }
}
