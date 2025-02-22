using DEDrake;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BoothDotDev.Data.Web.ValueConverters;

internal sealed class ShortGuidToStringConverter : ValueConverter<ShortGuid, string>
{
    public ShortGuidToStringConverter()
        : base(
            v => v.ToString(),
            v => ShortGuid.Parse(v))
    {
    }
}
