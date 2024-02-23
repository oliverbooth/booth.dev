using System.Text.Json;
using OliverBooth.Data;

namespace OliverBooth.Services;

internal sealed class MastodonService : IMastodonService
{
    private readonly HttpClient _httpClient;

    public MastodonService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public IMastodonStatus GetLatestStatus()
    {
        string token = Environment.GetEnvironmentVariable("MASTODON_TOKEN") ?? string.Empty;
        string account = Environment.GetEnvironmentVariable("MASTODON_ACCOUNT") ?? string.Empty;
        using var request = new HttpRequestMessage();
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.RequestUri = new Uri($"https://mastodon.olivr.me/api/v1/accounts/{account}/statuses");

        using HttpResponseMessage response = _httpClient.Send(request);
        using var stream = response.Content.ReadAsStream();
        var statuses = JsonSerializer.Deserialize<MastodonStatus[]>(stream) ?? Array.Empty<MastodonStatus>();
        return statuses[0];
    }
}
