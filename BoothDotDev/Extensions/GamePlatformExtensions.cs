using BoothDotDev.Data;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extensions for <see cref="GamePlatform" />.
/// </summary>
public static class GamePlatformExtensions
{
    /// <param name="platform">The platform.</param>
    extension(GamePlatform platform)
    {
        /// <summary>
        ///     Gets the name of the platform.
        /// </summary>
        /// <value>The name, such as <c>SNES</c> or <c>Mega Drive</c>.</value>
        public string DisplayName
        {
            get => platform switch
            {
                GamePlatform.Nes => "NES",
                GamePlatform.Snes => "SNES",
                GamePlatform.N64 => "N64",
                GamePlatform.GameCube => "GameCube",
                GamePlatform.Wii => "Wii",
                GamePlatform.WiiU => "Wii U",
                GamePlatform.Switch => "Switch",
                GamePlatform.GameBoy => "Game Boy",
                GamePlatform.GameBoyAdvance => "GBA",
                GamePlatform.NintendoDs => "DS",
                GamePlatform.PlayStation => "PlayStation",
                GamePlatform.Xbox => "Xbox",
                GamePlatform.MasterSystem => "Master System",
                GamePlatform.MegaDrive => "Mega Drive",
                GamePlatform.Saturn => "Saturn",
                GamePlatform.Dreamcast => "Dreamcast",
                GamePlatform.GameGear => "Game Gear",
                GamePlatform.Commodore64 => "Commodore 64",
                GamePlatform.Amiga => "Amiga",
                GamePlatform.Atari2600 => "Atari 2600",
                GamePlatform.ZxSpectrum => "ZX Spectrum",
                GamePlatform.AmstradCpc => "Amstrad CPC",
                GamePlatform.NeoGeo => "Neo Geo",
                GamePlatform.Arcade => "Arcade",
                GamePlatform.Steam => "Steam",
                GamePlatform.SteamDeck => "Steam Deck",
                GamePlatform.EpicGames => "Epic Games",
                GamePlatform.Gog => "GOG",
                GamePlatform.ItchIo => "itch.io",
                GamePlatform.GameJolt => "GameJolt",
                GamePlatform.Pc => "PC",
                GamePlatform.Android => "Android",
                GamePlatform.Ios => "iOS",
                GamePlatform.Other => "Other",
                _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
            };
        }

        /// <summary>
        ///     Gets the file name, in <c>/img/brand</c>, of the logo of the platform, its maker or its store.
        /// </summary>
        /// <value>The file name, or <see langword="null" /> if the platform has none and uses <see cref="Icon" /> instead.</value>
        public string? BrandGlyph
        {
            get => platform switch
            {
                GamePlatform.Switch => "switch.svg",
                GamePlatform.GameCube => "gamecube.svg",
                GamePlatform.PlayStation => "playstation.svg",
                GamePlatform.Xbox => "xbox.svg",
                GamePlatform.Dreamcast => "dreamcast.svg",
                GamePlatform.MasterSystem or GamePlatform.MegaDrive or GamePlatform.Saturn or GamePlatform.GameGear => "sega.svg",
                GamePlatform.Commodore64 or GamePlatform.Amiga => "commodore.svg",
                GamePlatform.Atari2600 => "atari.svg",
                GamePlatform.Steam => "steam.svg",
                GamePlatform.SteamDeck => "steamdeck.svg",
                GamePlatform.EpicGames => "epicgames.svg",
                GamePlatform.Gog => "gog.svg",
                GamePlatform.ItchIo => "itchio.svg",
                GamePlatform.GameJolt => "gamejolt.svg",
                GamePlatform.Android => "googleplay.svg",
                GamePlatform.Ios => "apple.svg",
                _ => null
            };
        }

        /// <summary>
        ///     Gets the name of the Tabler icon shown for a platform that has no <see cref="BrandGlyph" />, without its
        ///     <c>ti-</c> prefix.
        /// </summary>
        /// <value>The icon name.</value>
        public string Icon
        {
            get => platform switch
            {
                GamePlatform.Pc or GamePlatform.ZxSpectrum or GamePlatform.AmstradCpc => "device-desktop",
                _ => "device-gamepad-2"
            };
        }

        /// <summary>
        ///     Gets the heading the platform is grouped under when picking platforms.
        /// </summary>
        /// <value>The group name, such as <c>Nintendo</c>.</value>
        public string Group
        {
            get => platform switch
            {
                GamePlatform.Nes or GamePlatform.Snes or GamePlatform.N64 or GamePlatform.GameCube or GamePlatform.Wii or GamePlatform.WiiU or GamePlatform.Switch or GamePlatform.GameBoy or GamePlatform.GameBoyAdvance or GamePlatform.NintendoDs => "Nintendo",
                GamePlatform.PlayStation or GamePlatform.Xbox => "Sony & Microsoft",
                GamePlatform.MasterSystem or GamePlatform.MegaDrive or GamePlatform.Saturn or GamePlatform.Dreamcast or GamePlatform.GameGear => "Sega",
                GamePlatform.Commodore64 or GamePlatform.Amiga or GamePlatform.Atari2600 or GamePlatform.ZxSpectrum or GamePlatform.AmstradCpc or GamePlatform.NeoGeo or GamePlatform.Arcade => "Retro & arcade",
                GamePlatform.Steam or GamePlatform.SteamDeck or GamePlatform.EpicGames or GamePlatform.Gog or GamePlatform.ItchIo or GamePlatform.GameJolt or GamePlatform.Pc => "PC & stores",
                GamePlatform.Android or GamePlatform.Ios => "Mobile",
                GamePlatform.Other => "Other",
                _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
            };
        }
    }
}
