using System.Text.Json;
using BoothDotDev.Data.Discord;

namespace BoothDotDev.Tests;

[TestFixture]
internal sealed class DiscordComponentEmbedTests
{
    private static JsonElement Serialize(DiscordComponentEmbed embed)
    {
        return JsonDocument.Parse(embed.ToJson()).RootElement;
    }

    private static DiscordComponentEmbed Sample(string title = "Title")
    {
        return new DiscordComponentEmbed(new DiscordContainer(
            0x6161CD,
            [
                new DiscordTextDisplay($"# **{title}**"),
                new DiscordMediaGallery([new DiscordMediaGalleryItem(new DiscordMedia("https://example.com/card.png"))]),
                new DiscordSeparator(),
                new DiscordActionRow([new DiscordButton("https://example.com/post", "Read post")])
            ]));
    }

    [Test]
    public void ToJson_RootIsAContainerWithAccentColor()
    {
        var component = Serialize(Sample()).GetProperty("component");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(component.GetProperty("type").GetInt32(), Is.EqualTo(17));
            Assert.That(component.GetProperty("accent_color").GetInt32(), Is.EqualTo(0x6161CD));
        }
    }

    [Test]
    public void ToJson_EveryComponentCarriesItsType()
    {
        var children = Serialize(Sample()).GetProperty("component").GetProperty("components");
        var buttons = children[3].GetProperty("components");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(children.EnumerateArray().Select(c => c.GetProperty("type").GetInt32()), Is.EqualTo(new[] { 10, 12, 14, 1 }));
            Assert.That(buttons[0].GetProperty("type").GetInt32(), Is.EqualTo(2));
        }
    }

    [Test]
    public void ToJson_ButtonHasOnlyKeysDiscordAllows()
    {
        var button = Serialize(Sample()).GetProperty("component").GetProperty("components")[3].GetProperty("components")[0];

        Assert.That(button.EnumerateObject().Select(p => p.Name), Is.EquivalentTo(new[] { "type", "url", "style", "label" }));
        Assert.That(button.GetProperty("style").GetInt32(), Is.EqualTo(5));
    }

    [Test]
    public void ToJson_MediaGalleryItemsNestTheUrlUnderMedia()
    {
        var gallery = Serialize(Sample()).GetProperty("component").GetProperty("components")[1];

        Assert.That(gallery.GetProperty("items")[0].GetProperty("media").GetProperty("url").GetString(),
            Is.EqualTo("https://example.com/card.png"));
    }

    [Test]
    public void ToJson_EscapesMarkupSoItCannotCloseTheScriptElement()
    {
        var json = Sample("</script><b>").ToJson();

        Assert.That(json, Does.Not.Contain("<").And.Not.Contain(">"));
    }
}
