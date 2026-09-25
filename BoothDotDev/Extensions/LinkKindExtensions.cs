using BoothDotDev.Data;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extensions for <see cref="LinkKind" />.
/// </summary>
public static class LinkKindExtensions
{
    /// <param name="kind">The kind of link.</param>
    extension(LinkKind kind)
    {
        /// <summary>
        ///     Gets the name of the place the kind of link leads to.
        /// </summary>
        /// <value>The name, such as <c>GitHub</c> or <c>itch.io</c>.</value>
        public string DisplayName
        {
            get => kind switch
            {
                LinkKind.GitHub => "GitHub",
                LinkKind.GitLab => "GitLab",
                LinkKind.Itch => "itch.io",
                LinkKind.PlayStore => "Google Play",
                LinkKind.Steam => "Steam",
                LinkKind.YouTube => "YouTube",
                LinkKind.Discord => "Discord",
                LinkKind.DeviantArt => "DeviantArt",
                LinkKind.Behance => "Behance",
                LinkKind.SoundCloud => "SoundCloud",
                LinkKind.Documentation => "Documentation",
                LinkKind.Website => "Website",
                LinkKind.Other => "Other",
                LinkKind.GameJolt => "GameJolt",
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }

        /// <summary>
        ///     Gets the file name, in <c>/img/brand</c>, of the brand's logo.
        /// </summary>
        /// <value>The file name, or <see langword="null" /> if the kind isn't a brand and uses <see cref="Icon" /> instead.</value>
        public string? BrandGlyph
        {
            get => kind switch
            {
                LinkKind.GitHub => "github.svg",
                LinkKind.GitLab => "gitlab.svg",
                LinkKind.Itch => "itchio.svg",
                LinkKind.PlayStore => "googleplay.svg",
                LinkKind.Steam => "steam.svg",
                LinkKind.YouTube => "youtube.svg",
                LinkKind.Discord => "discord.svg",
                LinkKind.DeviantArt => "deviantart.svg",
                LinkKind.Behance => "behance.svg",
                LinkKind.SoundCloud => "soundcloud.svg",
                LinkKind.GameJolt => "gamejolt.svg",
                _ => null
            };
        }

        /// <summary>
        ///     Gets the name of the Tabler icon shown on a link of a kind that has no <see cref="BrandGlyph" />, without its
        ///     <c>ti-</c> prefix.
        /// </summary>
        /// <value>The icon name.</value>
        public string Icon
        {
            get => kind switch
            {
                LinkKind.Documentation => "book",
                LinkKind.Website => "world",
                _ => "link"
            };
        }

        /// <summary>
        ///     Gets the text shown on a link of the kind when it has no label of its own.
        /// </summary>
        /// <value>The text, such as <c>Repository</c> or <c>Steam Page</c>.</value>
        public string DefaultText
        {
            get => kind switch
            {
                LinkKind.GitHub or LinkKind.GitLab => "Repository",
                LinkKind.Itch => "itch.io Page",
                LinkKind.PlayStore => "Google Play Store Page",
                LinkKind.Steam => "Steam Page",
                LinkKind.YouTube => "Watch on YouTube",
                LinkKind.Discord => "Join the Discord",
                LinkKind.DeviantArt => "View on DeviantArt",
                LinkKind.Behance => "View on Behance",
                LinkKind.SoundCloud => "Listen on SoundCloud",
                LinkKind.Documentation => "Documentation",
                LinkKind.Website => "External site",
                LinkKind.Other => "Link",
                LinkKind.GameJolt => "GameJolt Page",
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }
    }
}
