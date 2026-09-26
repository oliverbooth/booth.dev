using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoothDotDev.Data.Discord;

/// <summary>
///     Represents a component of a Discord component embed. The <c>type</c> value in the JSON comes from the derived
///     type, so a component can't be written with the wrong one.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(DiscordActionRow), 1)]
[JsonDerivedType(typeof(DiscordButton), 2)]
[JsonDerivedType(typeof(DiscordTextDisplay), 10)]
[JsonDerivedType(typeof(DiscordMediaGallery), 12)]
[JsonDerivedType(typeof(DiscordSeparator), 14)]
[JsonDerivedType(typeof(DiscordContainer), 17)]
public abstract record DiscordComponent;

/// <summary>
///     Represents the top-level component of a component embed.
/// </summary>
/// <param name="AccentColor">The colour of the bar beside the embed, as an sRGB integer.</param>
/// <param name="Components">The components inside the container.</param>
public sealed record DiscordContainer(int AccentColor, IReadOnlyList<DiscordComponent> Components) : DiscordComponent;

/// <summary>
///     Represents a block of Markdown text.
/// </summary>
/// <param name="Content">The Markdown.</param>
public sealed record DiscordTextDisplay(string Content) : DiscordComponent;

/// <summary>
///     Represents a set of images.
/// </summary>
/// <param name="Items">The images.</param>
public sealed record DiscordMediaGallery(IReadOnlyList<DiscordMediaGalleryItem> Items) : DiscordComponent;

/// <summary>
///     Represents one image in a <see cref="DiscordMediaGallery" />.
/// </summary>
/// <param name="Media">The image.</param>
public sealed record DiscordMediaGalleryItem(DiscordMedia Media);

/// <summary>
///     Represents an image by its address.
/// </summary>
/// <param name="Url">The absolute, public address of the image.</param>
public sealed record DiscordMedia(string Url);

/// <summary>
///     Represents a divider between components.
/// </summary>
public sealed record DiscordSeparator : DiscordComponent;

/// <summary>
///     Represents a row of buttons.
/// </summary>
public sealed record DiscordActionRow : DiscordComponent
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DiscordActionRow" /> class.
    /// </summary>
    /// <param name="buttons">The buttons.</param>
    public DiscordActionRow(IEnumerable<DiscordButton> buttons)
    {
        Components = [.. buttons];
    }

    /// <summary>
    ///     Gets the buttons. They are typed as components so each is written with its <c>type</c>.
    /// </summary>
    /// <value>The buttons.</value>
    public IReadOnlyList<DiscordComponent> Components { get; }
}

/// <summary>
///     Represents a button that opens a link. Discord rejects the whole embed if a button has any other kind of behaviour, so
///     this is the only kind there is.
/// </summary>
/// <param name="Url">The absolute address the button opens.</param>
/// <param name="Label">The text on the button.</param>
public sealed record DiscordButton(string Url, string Label) : DiscordComponent
{
    /// <summary>
    ///     Gets the button style, which is always the link style.
    /// </summary>
    /// <value>The style.</value>
    public int Style
    {
        get => 5;
    }
}

/// <summary>
///     Represents the payload of a component embed, as it appears in the page.
/// </summary>
/// <param name="Component">The container that holds the whole embed.</param>
public sealed record DiscordComponentEmbed(DiscordComponent Component)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, Encoder = JavaScriptEncoder.Default
    };

    /// <summary>
    ///     Converts the embed to JSON.
    /// </summary>
    /// <returns>The JSON, in which <c>&lt;</c> and <c>&gt;</c> are escaped so it is safe inside a <c>&lt;script&gt;</c> element.</returns>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, SerializerOptions);
    }
}
