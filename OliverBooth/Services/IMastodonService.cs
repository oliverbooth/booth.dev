using OliverBooth.Data.Mastodon;

namespace OliverBooth.Services;

public interface IMastodonService
{
    /// <summary>
    ///     Gets the latest status posted to Mastodon.
    /// </summary>
    /// <returns>The latest status.</returns>
    MastodonStatus GetLatestStatus();
}