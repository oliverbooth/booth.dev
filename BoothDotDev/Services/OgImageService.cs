using System.Reflection;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for rendering branded Open Graph preview images, matching the site's dark theme
///     (<c>src/css/partials/_tokens.css</c>).
/// </summary>
public sealed class OgImageService
{
    /// <summary>
    ///     Identifies the current rendering logic/layout, folded into the cache path <see cref="Controllers.OgImageController" />
    ///     writes generated cards under. Content-based cache invalidation (comparing a cached file's age against the content's
    ///     own <c>UpdatedAt</c>) has no way to know the *template* changed rather than the content - bump this whenever
    ///     <see cref="DrawCard" /> or its layout changes, so previously-cached cards stop being served stale.
    /// </summary>
    public const string TemplateVersion = "v1";

    /// <summary>
    ///     The pixel width every rendered card is encoded at, exposed for the <c>og:image:width</c> meta tag.
    /// </summary>
    public const int Width = 1200;

    /// <summary>
    ///     The pixel height every rendered card is encoded at, exposed for the <c>og:image:height</c> meta tag.
    /// </summary>
    public const int Height = 630;

    private const int Margin = 72;
    private const int AccentBarWidth = 6;
    private const int FullWrapLength = Width - Margin - Margin;

    // Photo cards show the image full-bleed (cropping it into a narrow side panel guillotines wide hero art that
    // has its own text/wordmark baked in, e.g. a banner reading "BREAKOUT I GUESS" across its full width) and rely
    // on a poster-style top-to-bottom gradient - image fully clear up top, opaque toward the bottom where the text
    // sits - rather than a uniform scrim, since no single flat opacity reads well against every possible backdrop.
    private const int GradientTransparentEndY = 180;
    private const int GradientOpaqueStartY = 380;
    private const int PhotoWrapLength = Width - Margin - Margin - 96;
    private const float PhotoMaxDescriptionHeight = 90f;

    private const int MaxTitleHeight = 220;
    private const float MinTitleFontSize = 32f;
    private const float MaxTitleFontSize = 56f;

    private static readonly Color BackgroundColor = Color.ParseHex("0d0d10");
    private static readonly Color AccentColor = Color.ParseHex("6161cd");
    private static readonly Color TextPrimaryColor = Color.ParseHex("f2f2f4");
    private static readonly Color TextSecondaryColor = Color.ParseHex("9a9aa5");
    private readonly Font _descriptionFont;
    private readonly Font _eyebrowFont;

    private readonly FontFamily _interSemiBold;
    private readonly Font _wordmarkFont;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OgImageService" /> class, loading the embedded fonts once.
    /// </summary>
    public OgImageService()
    {
        var collection = new FontCollection();
        var assembly = typeof(OgImageService).Assembly;

        var interRegular = AddFont(collection, assembly, "Inter-Regular.ttf");
        _interSemiBold = AddFont(collection, assembly, "Inter-SemiBold.ttf");
        var mono = AddFont(collection, assembly, "JetBrainsMono-Regular.ttf");

        _eyebrowFont = mono.CreateFont(20, FontStyle.Regular);
        _descriptionFont = interRegular.CreateFont(28, FontStyle.Regular);
        _wordmarkFont = mono.CreateFont(20, FontStyle.Regular);
    }

    /// <summary>
    ///     Renders a flat branded card with no backdrop image, spanning the full width.
    /// </summary>
    /// <param name="eyebrow">The content-type label shown above the title (e.g. "BLOG POST").</param>
    /// <param name="title">The card's title.</param>
    /// <param name="description">The card's description/excerpt, or <see langword="null" /> to omit it.</param>
    /// <returns>The rendered card, encoded as PNG.</returns>
    public byte[] RenderFlatCard(string eyebrow, string title, string? description)
    {
        using var image = new Image<Rgba32>(Width, Height);
        image.Mutate(ctx => ctx.Fill(BackgroundColor));
        DrawCard(image, eyebrow, title, description, FullWrapLength);
        return Encode(image);
    }

    /// <summary>
    ///     Renders a poster-style card for content that has a real image (a project's hero image, an artwork item's
    ///     file): the image full-bleed, clear at the top, with a gradient darkening toward the bottom where the text
    ///     sits, so it stays legible regardless of what the image looks like.
    /// </summary>
    /// <param name="eyebrow">The content-type label shown above the title (e.g. "PROJECT").</param>
    /// <param name="title">The card's title.</param>
    /// <param name="description">The card's description/excerpt, or <see langword="null" /> to omit it.</param>
    /// <param name="backdropImagePath">The physical path of the image to use as the backdrop.</param>
    /// <returns>The rendered card, encoded as PNG.</returns>
    public byte[] RenderPhotoCard(string eyebrow, string title, string? description, string backdropImagePath)
    {
        using var image = Image.Load<Rgba32>(backdropImagePath);
        image.Mutate(ctx => ctx.Resize(new ResizeOptions { Size = new Size(Width, Height), Mode = ResizeMode.Crop }));

        var gradient = new LinearGradientBrush(
            new PointF(0, 0),
            new PointF(0, Height),
            GradientRepetitionMode.None,
            new ColorStop(0f, BackgroundColor.WithAlpha(0f)),
            new ColorStop((float)GradientTransparentEndY / Height, BackgroundColor.WithAlpha(0f)),
            new ColorStop((float)GradientOpaqueStartY / Height, BackgroundColor),
            new ColorStop(1f, BackgroundColor));
        image.Mutate(ctx => ctx.Fill(gradient));

        DrawPhotoCardText(image, eyebrow, title, description);
        return Encode(image);
    }

    private void DrawCard(Image<Rgba32> image, string eyebrow, string title, string? description, int wrapLength)
    {
        const float titleY = Margin + 56;
        const float gapAfterTitle = 28;
        const float wordmarkRowHeight = 40;
        const float gapAboveWordmark = 32;

        var titleFont = FitTitleFont(title, wrapLength);
        var titleOptions = new RichTextOptions(titleFont)
        {
            Origin = new PointF(Margin, titleY), WrappingLength = wrapLength, LineSpacing = 1.15f
        };
        var titleBounds = TextMeasurer.MeasureSize(title, titleOptions);

        // The description flows immediately after wherever the (variable-height, 1-3 line) title actually ends,
        // rather than sitting at a fixed offset - otherwise a short title leaves an awkward gap, and a long one
        // collides with it.
        var descriptionY = titleY + titleBounds.Height + gapAfterTitle;
        var maxDescriptionHeight = Height - Margin - wordmarkRowHeight - gapAboveWordmark - descriptionY;

        image.Mutate(ctx =>
        {
            ctx.Fill(AccentColor, new RectangleF(0, 0, AccentBarWidth, Height));

            var eyebrowOptions = new RichTextOptions(_eyebrowFont) { Origin = new PointF(Margin, Margin) };
            ctx.DrawText(eyebrowOptions, eyebrow.ToUpperInvariant(), AccentColor);

            ctx.DrawText(titleOptions, title, TextPrimaryColor);

            if (!string.IsNullOrWhiteSpace(description) && maxDescriptionHeight > 0)
            {
                var fitted = FitDescriptionText(Truncate(description, 300), _descriptionFont, maxDescriptionHeight, wrapLength);
                var descriptionOptions = new RichTextOptions(_descriptionFont)
                {
                    Origin = new PointF(Margin, descriptionY), WrappingLength = wrapLength, LineSpacing = 1.3f
                };
                ctx.DrawText(descriptionOptions, fitted, TextSecondaryColor);
            }

            var wordmarkOptions = new RichTextOptions(_wordmarkFont) { Origin = new PointF(Margin, Height - Margin) };
            ctx.DrawText(wordmarkOptions, $"{Strings.MyName} · booth.dev", TextSecondaryColor);
        });
    }

    /// <summary>
    ///     Draws the eyebrow, title, description, and wordmark stacked upward from the bottom of the canvas - the
    ///     poster-style counterpart to <see cref="DrawCard" />, which flows top-down. The stack starts from the
    ///     wordmark's fixed position and grows upward so it always lands inside the gradient's opaque zone,
    ///     regardless of how many lines the title/description end up wrapping to.
    /// </summary>
    private void DrawPhotoCardText(Image<Rgba32> image, string eyebrow, string title, string? description)
    {
        const float gapAboveWordmark = 24;
        const float gapAboveDescription = 12;
        const float gapAboveTitle = 8;

        var titleFont = FitTitleFont(title, PhotoWrapLength);
        var titleTextOptions = new TextOptions(titleFont) { WrappingLength = PhotoWrapLength, LineSpacing = 1.15f };
        var titleBounds = TextMeasurer.MeasureSize(title, titleTextOptions);

        var eyebrowTextOptions = new TextOptions(_eyebrowFont);
        var eyebrowBounds = TextMeasurer.MeasureSize(eyebrow, eyebrowTextOptions);

        string? fittedDescription = null;
        FontRectangle descriptionBounds = default;
        if (!string.IsNullOrWhiteSpace(description))
        {
            fittedDescription = FitDescriptionText(Truncate(description, 300), _descriptionFont, PhotoMaxDescriptionHeight,
                PhotoWrapLength);
            var descriptionTextOptions =
                new TextOptions(_descriptionFont) { WrappingLength = PhotoWrapLength, LineSpacing = 1.3f };
            descriptionBounds = TextMeasurer.MeasureSize(fittedDescription, descriptionTextOptions);
        }

        const float wordmarkY = Height - Margin;
        var descriptionY = wordmarkY - gapAboveWordmark - descriptionBounds.Height;
        var titleBottom = fittedDescription is null ? wordmarkY - gapAboveWordmark : descriptionY - gapAboveDescription;
        var titleY = titleBottom - titleBounds.Height;
        var eyebrowY = titleY - gapAboveTitle - eyebrowBounds.Height;

        image.Mutate(ctx =>
        {
            ctx.Fill(AccentColor, new RectangleF(0, 0, AccentBarWidth, Height));

            var eyebrowOptions = new RichTextOptions(_eyebrowFont) { Origin = new PointF(Margin, eyebrowY) };
            ctx.DrawText(eyebrowOptions, eyebrow.ToUpperInvariant(), AccentColor);

            var titleOptions = new RichTextOptions(titleFont)
            {
                Origin = new PointF(Margin, titleY), WrappingLength = PhotoWrapLength, LineSpacing = 1.15f
            };
            ctx.DrawText(titleOptions, title, TextPrimaryColor);

            if (fittedDescription is not null)
            {
                var descriptionOptions = new RichTextOptions(_descriptionFont)
                {
                    Origin = new PointF(Margin, descriptionY), WrappingLength = PhotoWrapLength, LineSpacing = 1.3f
                };
                ctx.DrawText(descriptionOptions, fittedDescription, TextSecondaryColor);
            }

            var wordmarkOptions = new RichTextOptions(_wordmarkFont) { Origin = new PointF(Margin, wordmarkY) };
            ctx.DrawText(wordmarkOptions, $"{Strings.MyName} · booth.dev", TextSecondaryColor);
        });
    }

    /// <summary>
    ///     Truncates <paramref name="text" />, word by word, until its wrapped height fits within
    ///     <paramref name="maxHeight" /> - the description-side counterpart to <see cref="FitTitleFont" />, which
    ///     shrinks the font instead of the text since a title has no natural place to cut.
    /// </summary>
    private static string FitDescriptionText(string text, Font font, float maxHeight, int wrapLength)
    {
        var candidate = text.Trim();
        while (candidate.Length > 0)
        {
            var options = new TextOptions(font) { WrappingLength = wrapLength, LineSpacing = 1.3f };
            var bounds = TextMeasurer.MeasureSize(candidate, options);
            if (bounds.Height <= maxHeight)
            {
                return candidate;
            }

            var trimmed = candidate.TrimEnd('…', ' ');
            var cut = Math.Max(trimmed.Length - 20, 0);
            candidate = $"{trimmed[..cut].TrimEnd()}…";
        }

        return string.Empty;
    }

    /// <summary>
    ///     Picks the largest title font size, within the configured range, whose wrapped text still fits within
    ///     <see cref="MaxTitleHeight" /> - so a short title renders big, and a long one shrinks to fit rather than
    ///     overflowing into the description.
    /// </summary>
    private Font FitTitleFont(string title, int wrapLength)
    {
        for (var size = MaxTitleFontSize; size > MinTitleFontSize; size -= 4f)
        {
            var candidate = _interSemiBold.CreateFont(size, FontStyle.Regular);
            var options = new TextOptions(candidate) { WrappingLength = wrapLength, LineSpacing = 1.15f };
            var bounds = TextMeasurer.MeasureSize(title, options);
            if (bounds.Height <= MaxTitleHeight)
            {
                return candidate;
            }
        }

        return _interSemiBold.CreateFont(MinTitleFontSize, FontStyle.Regular);
    }

    private static string Truncate(string text, int maxLength)
    {
        text = text.Trim();
        return text.Length <= maxLength ? text : $"{text[..maxLength].TrimEnd()}…";
    }

    private static FontFamily AddFont(FontCollection collection, Assembly assembly, string fileName)
    {
        var resourceName = $"BoothDotDev.Resources.Fonts.{fileName}";
        using var stream = assembly.GetManifestResourceStream(resourceName)
                           ?? throw new InvalidOperationException($"Embedded font resource '{resourceName}' was not found.");
        return collection.Add(stream);
    }

    private static byte[] Encode(Image<Rgba32> image)
    {
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
