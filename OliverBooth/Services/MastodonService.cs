using System.Text.Json;
using System.Text.Json.Serialization;
using HtmlAgilityPack;
using OliverBooth.Data.Mastodon;

namespace OliverBooth.Services;

internal sealed class MastodonService : IMastodonService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly HttpClient _httpClient;

    public MastodonService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public MastodonStatus GetLatestStatus()
    {
        string token = Environment.GetEnvironmentVariable("MASTODON_TOKEN") ?? string.Empty;
        string account = Environment.GetEnvironmentVariable("MASTODON_ACCOUNT") ?? string.Empty;
        using var request = new HttpRequestMessage();
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.RequestUri = new Uri($"https://mastodon.olivr.me/api/v1/accounts/{account}/statuses");

        using HttpResponseMessage response = _httpClient.Send(request);
        using var stream = response.Content.ReadAsStream();
        var statuses = JsonSerializer.Deserialize<MastodonStatus[]>(stream, JsonSerializerOptions);

        MastodonStatus status = statuses?[0]!;
        var document = new HtmlDocument();
        document.LoadHtml(status.Content);

        HtmlNodeCollection? links = document.DocumentNode.SelectNodes("//a");
        if (links is not null)
        {
            foreach (HtmlNode link in links)
            {
                link.InnerHtml = link.InnerText;
            }
        }

        status.Content = document.DocumentNode.OuterHtml;
        return status;
    }
}
