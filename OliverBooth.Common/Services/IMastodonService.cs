using OliverBooth.Common.Data.Mastodon;

namespace OliverBooth.Common.Services;

public interface IMastodonService
{
    /// <summary>
    ///     Gets the latest status posted to Mastodon.
    /// </summary>
    /// <returns>The latest status.</returns>
    IMastodonStatus GetLatestStatus();
}