using System.Text.Json;
using BoothDotDev.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for interacting with Bluesky.
/// </summary>
/// <param name="httpClient">The <see cref="HttpClient" /> to use for making requests to the Bluesky API.</param>
/// <param name="cache">The <see cref="IMemoryCache" /> to use for caching the latest post.</param>
/// <param name="options">
///     The <see cref="IOptionsMonitor{BlueskyOptions}" /> to use for accessing configuration options.
/// </param>
public sealed class BlueskyService(
    HttpClient httpClient,
    IMemoryCache cache,
    IOptionsMonitor<BlueskyOptions> options)
{
    private const string CacheKey = "bluesky_latest_post";
    private const string BaseUrl = "https://public.api.bsky.app/xrpc/app.bsky.feed.getAuthorFeed";

    /// <summary>
    ///     Gets the latest Bluesky post.
    /// </summary>
    /// <returns>The latest Bluesky post, or <see langword="null" /> if no post was found.</returns>
    public async Task<BlueskyPost?> GetLatestPostAsync()
    {
        if (cache.TryGetValue(CacheKey, out BlueskyPost? cached))
        {
            return cached;
        }

        var opts = options.CurrentValue;
        var url = $"{BaseUrl}?actor={Uri.EscapeDataString(opts.Handle)}&filter=posts_no_replies&limit=10";

        using var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        var feed = doc.RootElement.GetProperty("feed");

        foreach (var item in feed.EnumerateArray())
        {
            var post = item.GetProperty("post");

            // Skip reposts if configured to do so
            if (!opts.IncludeReposts && item.TryGetProperty("reason", out var reason))
            {
                var reasonType = reason.GetProperty("$type").GetString();
                if (reasonType == "app.bsky.feed.defs#reasonRepost")
                {
                    continue;
                }
            }

            var uri = post.GetProperty("uri").GetString()!;
            // AT-URI: at://did:plc:.../app.bsky.feed.post/{rkey}
            var rkey = uri.Split('/').Last(); 
            var atUri = uri["at://".Length..]; // strips "at://" prefix
            var postUrl = $"https://bsky.app/profile/{opts.Handle}/post/{rkey}"; // keep for linking

            var result = new BlueskyPost(atUri, postUrl);
            cache.Set(CacheKey, result, TimeSpan.FromMinutes(opts.CacheDurationMinutes));
            return result;
        }

        return null;
    }
}

/// <summary>
///     Represents a Bluesky post.
/// </summary>
/// <param name="AtUri">The AT-URI of the post.</param>
/// <param name="PostUrl">The URL to view the post on Bluesky.app.</param>
public sealed record BlueskyPost(string AtUri, string PostUrl);
