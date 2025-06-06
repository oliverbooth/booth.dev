using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BoothDotDev.Data;

internal sealed class StringListConverter : ValueConverter<IReadOnlyList<string>, string>
{
    public StringListConverter(char separator = ' ') :
        base(v => string.Join(separator, v),
            s => s.Split(separator, StringSplitOptions.None))
    {
    }
}
