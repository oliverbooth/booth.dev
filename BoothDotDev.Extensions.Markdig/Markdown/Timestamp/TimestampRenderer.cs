using System.ComponentModel;
using Humanizer;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace BoothDotDev.Extensions.Markdig.Markdown.Timestamp;

/// <summary>
///     Represents a Markdown object renderer that renders <see cref="TimestampInline" /> elements.
/// </summary>
public sealed class TimestampRenderer : HtmlObjectRenderer<TimestampInline>
{
    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, TimestampInline obj)
    {
        DateTimeOffset timestamp = obj.Timestamp;
        TimestampFormat format = obj.Format;

        renderer.Write("<span class=\"timestamp\" data-timestamp=\"");
        renderer.Write(timestamp.ToUnixTimeSeconds().ToString());
        renderer.Write("\" data-format=\"");
        renderer.Write(((char)format).ToString());
        renderer.Write("\" title=\"");
        renderer.WriteEscape(timestamp.ToString("dddd, d MMMM yyyy HH:mm"));
        renderer.Write("\">");

        switch (format)
        {
            case TimestampFormat.LongDate:
                renderer.Write(timestamp.ToString("d MMMM yyyy"));
                break;

            case TimestampFormat.LongDateShortTime:
                renderer.Write(timestamp.ToString(@"d MMMM yyyy \a\t HH:mm"));
                break;

            case TimestampFormat.LongDateTime:
                renderer.Write(timestamp.ToString(@"dddd, d MMMM yyyy \a\t HH:mm"));
                break;

            case TimestampFormat.Relative:
                renderer.Write(timestamp.Humanize());
                break;

            case var _ when !Enum.IsDefined(format):
                throw new InvalidEnumArgumentException(nameof(format), (int)format, typeof(TimestampFormat));

            default:
                renderer.Write(timestamp.ToString(((char)format).ToString()));
                break;
        }

        renderer.Write("</span>");
    }
}
