using System.Globalization;
using SmartFormat.Core.Extensions;

namespace OliverBooth;

/// <summary>
///     Represents a SmartFormat formatter that formats a date.
/// </summary>
internal sealed class DateFormatter : IFormatter
{
    /// <inheritdoc />
    public bool CanAutoDetect { get; set; } = true;

    /// <inheritdoc />
    public string Name { get; set; } = "date";

    /// <inheritdoc />
    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        if (formattingInfo.CurrentValue is not string value)
            return false;

        if (!DateTime.TryParseExact(value, "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime date))
            return false;


        formattingInfo.Write(date.ToString(formattingInfo.Format?.ToString()));
        return true;
    }
}
