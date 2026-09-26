using System.Reflection;
using BoothDotDev.Data;
using BoothDotDev.Extensions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for rendering branded Open Graph preview images, following the site's bright hue palette.
/// </summary>
public sealed class OgImageService
{
    /// <summary>
    ///     Identifies the current rendering logic/layout, folded into the cache path <see cref="Controllers.OgImageController" />
    ///     writes generated cards under. Content-based cache invalidation (comparing a cached file's age against the content's
    ///     own <c>UpdatedAt</c>) has no way to know the *template* changed rather than the content - bump this whenever
    ///     <see cref="RenderCard" /> or its layout changes, so previously-cached cards stop being served stale.
    /// </summary>
    public const string TemplateVersion = "v4";

    /// <summary>
    ///     The pixel width every rendered card is encoded at, exposed for the <c>og:image:width</c> meta tag.
    /// </summary>
    public const int Width = 1200;

    /// <summary>
    ///     The pixel height every rendered card is encoded at, exposed for the <c>og:image:height</c> meta tag.
    /// </summary>
    public const int Height = 630;

    private const int MarginX = 84;
    private const int MarginY = 78;
    private const int TextWidth = Width - MarginX - MarginX;
    private const int BadgePaddingX = 30;
    private const int BadgePaddingY = 12;
    private const int WordmarkIconSize = 48;
    private const int WordmarkGap = 18;
    private const float TitleToSubtitleGap = 26f;
    private const float MaxTitleHeight = 190f;
    private const float MinTitleFontSize = 44f;
    private const float MaxTitleFontSize = 66f;
    private const int MaxSubtitleLines = 2;
    private const float TitleLineHeight = 1.2f;
    private const float SubtitleLineHeight = 1.4f;

    private const int BrandIconCanvas = 1024;
    private const float BrandIconTilt = -4f;
    private const int WordmarkIconBleed = 2;

    private static readonly Color Brand = Color.FromRgb(0x61, 0x61, 0xCD);

    private readonly Font _badgeFont;
    private readonly Font _subtitleFont;
    private readonly FontFamily _titleFamily;
    private readonly Font _wordmarkFont;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OgImageService" /> class, loading the embedded fonts once.
    /// </summary>
    public OgImageService()
    {
        var collection = new FontCollection();
        var assembly = typeof(OgImageService).Assembly;

        _titleFamily = AddFont(collection, assembly, "baloo-2-latin-800-normal.woff");
        var bold = AddFont(collection, assembly, "baloo-2-latin-700-normal.woff");
        var nunito = AddFont(collection, assembly, "nunito-latin-400-normal.woff");
        var mono = AddFont(collection, assembly, "JetBrainsMono-Regular.ttf");

        _badgeFont = mono.CreateFont(30);
        _subtitleFont = nunito.CreateFont(34);
        _wordmarkFont = bold.CreateFont(33);
    }

    /// <summary>
    ///     Gets the initials shown in the brand icon, taken from the site owner's name.
    /// </summary>
    /// <value>The initials, in upper case.</value>
    public static string Initials
    {
        get => $"{Strings.MyFirstName[0]}{Strings.MySurname[0]}".ToUpperInvariant();
    }

    /// <summary>
    ///     Renders the brand icon: a tilted, rounded, gradient tile carrying the initials, matching the theme toggle.
    /// </summary>
    /// <param name="initials">The letters to show on the tile.</param>
    /// <param name="size">The width and height of the icon in pixels.</param>
    /// <returns>The icon, encoded as PNG with a transparent background.</returns>
    public byte[] RenderBrandIcon(string initials, int size)
    {
        using var icon = CreateBrandIcon(initials, size);
        using var stream = new MemoryStream();
        icon.SaveAsPng(stream);
        return stream.ToArray();
    }

    /// <summary>
    ///     Renders a card: a gradient in the given hue, a badge, a title and subtitle, and the site's wordmark.
    /// </summary>
    /// <param name="hue">The hue the card is drawn in.</param>
    /// <param name="badge">The content-type label shown in the pill at the top (e.g. "POST").</param>
    /// <param name="title">The card's title.</param>
    /// <param name="subtitle">The line under the title, or <see langword="null" /> to omit it.</param>
    /// <returns>The rendered card, encoded as PNG.</returns>
    public byte[] RenderCard(PaletteHue hue, string badge, string title, string? subtitle)
    {
        using var image = new Image<Rgba32>(Width, Height);
        var accent = ColorOf(hue);

        // CSS's 135deg gradient runs corner to corner along a line longer than the box, so the end colours are reached
        // exactly at the corners
        image.Mutate(ctx => ctx.Fill(new LinearGradientBrush(
            new PointF(142.5f, -142.5f),
            new PointF(1057.5f, 772.5f),
            GradientRepetitionMode.None,
            new ColorStop(0f, accent),
            new ColorStop(1f, Brand))));

        image.Mutate(ctx =>
        {
            ctx.Fill(Color.White.WithAlpha(0.08f), new EllipsePolygon(1080, 120, 210));
            ctx.Fill(Color.White.WithAlpha(0.06f), new EllipsePolygon(930, 630, 150));
        });

        var badgeBottom = DrawBadge(image, hue, badge);
        DrawTitleBlock(image, title, subtitle, badgeBottom);
        DrawWordmark(image);

        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }

    private Image<Rgba32> CreateBrandIcon(string initials, int size)
    {
        const float sideRatio = 1f;
        const float cornerRatio = 14f / 42f;
        const float letterRatio = 0.47f;

        var canvas = new Image<Rgba32>(BrandIconCanvas, BrandIconCanvas);
        var side = BrandIconCanvas * sideRatio;
        var start = (BrandIconCanvas - side) / 2;
        var centre = BrandIconCanvas / 2f;

        canvas.Mutate(ctx => ctx.Fill(
            new LinearGradientBrush(
                new PointF(start, start),
                new PointF(start + side, start + side),
                GradientRepetitionMode.None,
                new ColorStop(0f, Brand),
                new ColorStop(1f, ColorOf(PaletteHue.Pink))),
            RoundedRectangle(start, start, side, side, side * cornerRatio)));

        var text = initials.ToUpperInvariant();
        var options = new RichTextOptions(_titleFamily.CreateFont(side * letterRatio));
        var bounds = TextMeasurer.MeasureBounds(text, options);
        options.Origin = new PointF(centre - (bounds.X + bounds.Width / 2), centre - (bounds.Y + bounds.Height / 2));

        canvas.Mutate(ctx =>
        {
            ctx.DrawText(options, text, Color.White);
            ctx.Rotate(BrandIconTilt);
            ctx.Resize(new ResizeOptions { Size = new Size(size, size), Mode = ResizeMode.Pad, Sampler = KnownResamplers.Lanczos3 });
        });

        return canvas;
    }

    private static Color ColorOf(PaletteHue hue)
    {
        var rgb = hue.ToRgb();
        return Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
    }

    private float DrawBadge(Image<Rgba32> image, PaletteHue hue, string badge)
    {
        var text = badge.ToUpperInvariant();
        var options = new TextOptions(_badgeFont);
        var size = TextMeasurer.MeasureSize(text, options);
        var width = size.Width + BadgePaddingX * 2;
        var height = size.Height + BadgePaddingY * 2;

        var ink = TextMeasurer.MeasureBounds(text, options);
        var caps = TextMeasurer.MeasureBounds("H", options);
        var origin = new PointF(
            MarginX + width / 2 - (ink.X + ink.Width / 2), MarginY + height / 2 - (caps.Y + caps.Height / 2));

        image.Mutate(ctx =>
        {
            ctx.Fill(BadgeColor(hue), RoundedRectangle(MarginX, MarginY, width, height, height / 2));
            ctx.DrawText(new RichTextOptions(_badgeFont) { Origin = origin }, text, Color.White);
        });

        return MarginY + height;
    }

    private void DrawTitleBlock(Image<Rgba32> image, string title, string? subtitle, float top)
    {
        var (titleFont, fittedTitle) = FitTitle(title);
        title = fittedTitle;
        var titleOptions = new RichTextOptions(titleFont)
        {
            WrappingLength = TextWidth, LineSpacing = LineSpacingFor(titleFont, TitleLineHeight)
        };
        var titleHeight = TextMeasurer.MeasureSize(title, titleOptions).Height;

        string? fitted = null;
        var subtitleHeight = 0f;
        var subtitleOptions = new RichTextOptions(_subtitleFont)
        {
            WrappingLength = TextWidth, LineSpacing = LineSpacingFor(_subtitleFont, SubtitleLineHeight)
        };
        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            fitted = FitText(subtitle, subtitleOptions, MaxSubtitleLines * _subtitleFont.Size * SubtitleLineHeight + 1);
            subtitleHeight = TextMeasurer.MeasureSize(fitted, subtitleOptions).Height;
        }

        // the block sits midway between the badge and the wordmark, like the mockup's space-between column
        var wordmarkTop = Height - MarginY - WordmarkIconSize;
        var blockHeight = titleHeight + (fitted is null ? 0 : TitleToSubtitleGap + subtitleHeight);
        var titleY = top + (wordmarkTop - top - blockHeight) / 2;

        titleOptions.Origin = new PointF(MarginX, titleY);
        using (var shadow = new Image<Rgba32>(Width, Height, Color.Transparent))
        {
            var shadowOptions = new RichTextOptions(titleOptions) { Origin = new PointF(MarginX, titleY + 6) };
            shadow.Mutate(ctx =>
            {
                ctx.DrawText(shadowOptions, title, Color.Black.WithAlpha(0.3f));
                ctx.GaussianBlur(15f);
            });
            image.Mutate(ctx => ctx.DrawImage(shadow, 1f));
        }

        image.Mutate(ctx => ctx.DrawText(titleOptions, title, Color.White));

        if (fitted is not null)
        {
            subtitleOptions.Origin = new PointF(MarginX, titleY + titleHeight + TitleToSubtitleGap);
            image.Mutate(ctx => ctx.DrawText(subtitleOptions, fitted, Color.White.WithAlpha(0.75f)));
        }
    }

    private void DrawWordmark(Image<Rgba32> image)
    {
        var top = Height - MarginY - WordmarkIconSize;
        var name = Strings.MyName.ToLowerInvariant();
        var bounds = TextMeasurer.MeasureBounds(name, new TextOptions(_wordmarkFont));

        // the tile is drawn inside a transparent margin so its tilted corners aren't clipped, so it is drawn larger than the
        // slot it sits in
        using var icon = CreateBrandIcon(Initials, WordmarkIconSize + WordmarkIconBleed * 2);

        image.Mutate(ctx =>
        {
            ctx.DrawImage(icon, new Point(MarginX - WordmarkIconBleed, top - WordmarkIconBleed), 1f);
            ctx.DrawText(
                new RichTextOptions(_wordmarkFont)
                {
                    Origin = new PointF(
                        MarginX + WordmarkIconSize + WordmarkGap - bounds.X, top + WordmarkIconSize / 2f - (bounds.Y + bounds.Height / 2))
                },
                name, Color.White);
        });
    }

    private (Font Font, string Title) FitTitle(string title)
    {
        for (var size = MaxTitleFontSize; size > MinTitleFontSize; size -= 4f)
        {
            var candidate = _titleFamily.CreateFont(size);
            var options = new TextOptions(candidate)
            {
                WrappingLength = TextWidth, LineSpacing = LineSpacingFor(candidate, TitleLineHeight)
            };
            if (TextMeasurer.MeasureSize(title, options).Height <= MaxTitleHeight)
            {
                return (candidate, title);
            }
        }

        var smallest = _titleFamily.CreateFont(MinTitleFontSize);
        var smallestOptions = new TextOptions(smallest)
        {
            WrappingLength = TextWidth, LineSpacing = LineSpacingFor(smallest, TitleLineHeight)
        };
        return (smallest, FitText(title, smallestOptions, MaxTitleHeight));
    }

    /// <summary>
    ///     Cuts <paramref name="text" />, word by word, until its wrapped height fits within <paramref name="maxHeight" />.
    /// </summary>
    private static string FitText(string text, TextOptions options, float maxHeight)
    {
        var candidate = text.Trim();
        while (candidate.Length > 0)
        {
            if (TextMeasurer.MeasureSize(candidate, options).Height <= maxHeight)
            {
                return candidate;
            }

            var trimmed = candidate.TrimEnd('…', ' ');
            var cut = trimmed.LastIndexOf(' ');
            candidate = cut > 0 ? $"{trimmed[..cut]}…" : string.Empty;
        }

        return string.Empty;
    }

    /// <summary>
    ///     Converts a CSS-style line height (a multiple of the font size) to the multiple of the font's own line pitch
    ///     that ImageSharp expects. The pitch is measured rather than read from the font's metrics, which don't match what
    ///     is drawn for every font.
    /// </summary>
    private static float LineSpacingFor(Font font, float cssLineHeight)
    {
        var options = new TextOptions(font);
        var pitch = TextMeasurer.MeasureSize("A\nA", options).Height - TextMeasurer.MeasureSize("A", options).Height;
        return cssLineHeight * font.Size / pitch;
    }

    private static IPath RoundedRectangle(float x, float y, float width, float height, float radius)
    {
        var builder = new PathBuilder();
        builder.MoveTo(new PointF(x + radius, y));
        builder.LineTo(new PointF(x + width - radius, y));
        builder.ArcTo(radius, radius, 0, false, true, new PointF(x + width, y + radius));
        builder.LineTo(new PointF(x + width, y + height - radius));
        builder.ArcTo(radius, radius, 0, false, true, new PointF(x + width - radius, y + height));
        builder.LineTo(new PointF(x + radius, y + height));
        builder.ArcTo(radius, radius, 0, false, true, new PointF(x, y + height - radius));
        builder.LineTo(new PointF(x, y + radius));
        builder.ArcTo(radius, radius, 0, false, true, new PointF(x + radius, y));
        builder.CloseFigure();
        return builder.Build();
    }

    // the mockup's translucent dark tint of each hue, so the pill reads on every gradient
    private static Color BadgeColor(PaletteHue hue)
    {
        return hue switch
        {
            PaletteHue.Grape => Color.ParseHex("442E5C").WithAlpha(0.5f),
            PaletteHue.Pink => Color.ParseHex("552847").WithAlpha(0.5f),
            PaletteHue.Tangerine => Color.ParseHex("5D2919").WithAlpha(0.5f),
            PaletteHue.Sun => Color.ParseHex("52471F").WithAlpha(0.5f),
            PaletteHue.Mint => Color.ParseHex("074631").WithAlpha(0.5f),
            PaletteHue.Sky => Color.ParseHex("07405A").WithAlpha(0.5f),
            _ => Color.ParseHex("252B4C").WithAlpha(0.55f)
        };
    }

    private static FontFamily AddFont(FontCollection collection, Assembly assembly, string fileName)
    {
        var resourceName = $"BoothDotDev.Resources.Fonts.{fileName}";
        using var stream = assembly.GetManifestResourceStream(resourceName)
                           ?? throw new InvalidOperationException($"Embedded font resource '{resourceName}' was not found.");
        return collection.Add(stream);
    }
}
