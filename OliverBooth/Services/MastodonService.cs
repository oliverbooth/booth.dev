using System.Text.Json;
using System.Text.Json.Serialization;
using HtmlAgilityPack;
using OliverBooth.Common.Data.Mastodon;
using OliverBooth.Common.Services;
using OliverBooth.Data.Mastodon;

namespace OliverBooth.Services;

internal sealed class MastodonService : IMastodonService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public MastodonService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public IMastodonStatus GetLatestStatus()
    {
        string token = _configuration.GetSection("Mastodon:Token").Value ?? string.Empty;
        string account = _configuration.GetSection("Mastodon:Account").Value ?? string.Empty;
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
