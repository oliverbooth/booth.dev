using NpgsqlTypes;

namespace BoothDotDev.Data;

/// <summary>
///     An enumeration of the kinds of place a link can lead to.
/// </summary>
public enum LinkKind
{
    /// <summary>
    ///     A GitHub repository or page.
    /// </summary>
    [PgName("github")] GitHub,

    /// <summary>
    ///     A GitLab repository or page.
    /// </summary>
    [PgName("gitlab")] GitLab,

    /// <summary>
    ///     A page on itch.io.
    /// </summary>
    [PgName("itch")] Itch,

    /// <summary>
    ///     A listing on Google Play.
    /// </summary>
    [PgName("play_store")] PlayStore,

    /// <summary>
    ///     A page on Steam.
    /// </summary>
    [PgName("steam")] Steam,

    /// <summary>
    ///     A video or channel on YouTube.
    /// </summary>
    [PgName("youtube")] YouTube,

    /// <summary>
    ///     A Discord invite.
    /// </summary>
    [PgName("discord")] Discord,

    /// <summary>
    ///     A page on DeviantArt.
    /// </summary>
    [PgName("deviantart")] DeviantArt,

    /// <summary>
    ///     A page on Behance.
    /// </summary>
    [PgName("behance")] Behance,

    /// <summary>
    ///     A track or profile on SoundCloud.
    /// </summary>
    [PgName("soundcloud")] SoundCloud,

    /// <summary>
    ///     Documentation.
    /// </summary>
    [PgName("documentation")] Documentation,

    /// <summary>
    ///     A website.
    /// </summary>
    [PgName("website")] Website,

    /// <summary>
    ///     Anywhere else.
    /// </summary>
    [PgName("other")] Other,

    /// <summary>
    ///     A page on GameJolt.
    /// </summary>
    [PgName("gamejolt")] GameJolt
}
