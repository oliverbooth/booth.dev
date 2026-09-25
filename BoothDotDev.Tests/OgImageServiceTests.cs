using BoothDotDev.Data;
using BoothDotDev.Services;
using SixLabors.ImageSharp;

namespace BoothDotDev.Tests;

[TestFixture]
internal sealed class OgImageServiceTests
{
    private static readonly OgImageService Service = new();

    [TestCaseSource(nameof(Hues))]
    public void RenderCard_EveryHue_ProducesACardOfTheExpectedSize(PaletteHue hue)
    {
        var png = Service.RenderCard(hue, "POST", "Why I stopped caching everything", "A postmortem on a cache bug.");
        var info = Image.Identify(png);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(info.Width, Is.EqualTo(OgImageService.Width));
            Assert.That(info.Height, Is.EqualTo(OgImageService.Height));
        }
    }

    [Test]
    public void RenderCard_NoSubtitle_StillRenders()
    {
        Assert.That(Service.RenderCard(PaletteHue.Mint, "NOTE", "A title", null), Is.Not.Empty);
    }

    [Test]
    public void RenderCard_VeryLongTitleAndSubtitle_StillRenders()
    {
        var title = string.Join(' ', Enumerable.Repeat("extraordinarily", 20));
        var subtitle = string.Join(' ', Enumerable.Repeat("word", 400));

        Assert.That(Service.RenderCard(PaletteHue.Pink, "CHALLENGE", title, subtitle), Is.Not.Empty);
    }

    [Test]
    public void RenderCard_WhenPreviewDirectoryIsSet_WritesSamples()
    {
        var directory = Environment.GetEnvironmentVariable("OG_CARD_PREVIEW_DIR");
        if (string.IsNullOrEmpty(directory))
        {
            Assert.Ignore("OG_CARD_PREVIEW_DIR is not set.");
        }

        Directory.CreateDirectory(directory);
        var samples = new (PaletteHue Hue, string Badge, string Title, string? Subtitle)[]
        {
            (PaletteHue.Brand, "POST", "Why I stopped caching everything", "A postmortem on a cache bug, and what changed after."),
            (PaletteHue.Mint, "TUTORIAL", "TCP handshakes, three steps at a time", "Networking · part 2 of 5"),
            (PaletteHue.Pink, "CHALLENGE", "Rate Limiter From Scratch", "Implement a sliding-window rate limiter with no external libraries."),
            (PaletteHue.Sun, "NOTE", "Postgres connection pooling gotcha", "a quick note · Sep 2026"),
            (PaletteHue.Sky, "LIBRARY", "X10D", "A NuGet offering dozens of extension methods for countless .NET types."),
            (PaletteHue.Grape, "DRAWING", "An extraordinarily long title that goes on and on across several lines of the card just to see what happens",
                string.Join(' ', Enumerable.Repeat("A very long description sentence.", 12))),
            (PaletteHue.Tangerine, "3D", "Blender Guru: The Donut", null)
        };

        for (var i = 0; i < samples.Length; i++)
        {
            var (hue, badge, title, subtitle) = samples[i];
            File.WriteAllBytes(Path.Combine(directory, $"{i}-{hue}.png"), Service.RenderCard(hue, badge, title, subtitle));
        }

        foreach (var hue in Enum.GetValues<PaletteHue>())
        {
            File.WriteAllBytes(Path.Combine(directory, $"same-text-{hue}.png"),
                Service.RenderCard(hue, "POST", "Why I stopped caching everything", "A postmortem on a cache bug, and what changed after."));
        }
    }

    private static IEnumerable<PaletteHue> Hues()
    {
        return Enum.GetValues<PaletteHue>();
    }
}
