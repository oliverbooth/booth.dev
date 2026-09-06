using System.Text.Json;
using BoothDotDev.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for fetching the current weather for the /now page, via Open-Meteo.
/// </summary>
/// <param name="httpClient">The <see cref="HttpClient" /> to use for making requests to Open-Meteo.</param>
/// <param name="cache">The <see cref="IMemoryCache" /> to use for caching the current weather.</param>
/// <param name="options">The weather options.</param>
public sealed class WeatherService(HttpClient httpClient, IMemoryCache cache, IOptionsMonitor<WeatherOptions> options)
{
    private const string CacheKey = "weather_current";
    private const string BaseUrl = "https://api.open-meteo.com/v1/forecast";

    /// <summary>
    ///     Gets the current weather at the configured location.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>The current weather, or <see langword="null" /> if it isn't configured or couldn't be fetched.</returns>
    public async Task<WeatherSnapshot?> GetCurrentWeatherAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(CacheKey, out WeatherSnapshot? cached))
        {
            return cached;
        }

        var opts = options.CurrentValue;
        if (opts.Latitude == 0 && opts.Longitude == 0)
        {
            // (0, 0) is open ocean off the coast of Africa - nobody's real location, and the default for an unset
            // config value, so it doubles as the "not configured" sentinel.
            return null;
        }

        var url = $"{BaseUrl}?latitude={opts.Latitude}&longitude={opts.Longitude}&current=temperature_2m,weather_code";

        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(url, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var current = document.RootElement.GetProperty("current");
            var temperature = current.GetProperty("temperature_2m").GetDouble();
            var code = current.GetProperty("weather_code").GetInt32();

            var (description, icon) = DescribeWeatherCode(code);
            var snapshot = new WeatherSnapshot(temperature, description, icon);

            cache.Set(CacheKey, snapshot, TimeSpan.FromMinutes(opts.CacheDurationMinutes));
            return snapshot;
        }
    }

    /// <summary>
    ///     Maps a WMO weather interpretation code to a human-readable description and a Tabler icon name.
    /// </summary>
    /// <param name="code">The WMO weather code.</param>
    /// <returns>The description and icon.</returns>
    private static (string Description, string Icon) DescribeWeatherCode(int code)
    {
        return code switch
        {
            0 => ("Clear sky", "sun"),
            1 => ("Mainly clear", "sun"),
            2 => ("Partly cloudy", "cloud-sun"),
            3 => ("Overcast", "cloud"),
            45 or 48 => ("Fog", "cloud-fog"),
            51 or 53 or 55 => ("Drizzle", "cloud-drizzle"),
            56 or 57 => ("Freezing drizzle", "cloud-drizzle"),
            61 or 63 or 65 => ("Rain", "cloud-rain"),
            66 or 67 => ("Freezing rain", "cloud-rain"),
            71 or 73 or 75 => ("Snow", "snowflake"),
            77 => ("Snow grains", "snowflake"),
            80 or 81 or 82 => ("Rain showers", "cloud-rain"),
            85 or 86 => ("Snow showers", "snowflake"),
            95 => ("Thunderstorm", "cloud-lightning"),
            96 or 99 => ("Thunderstorm with hail", "cloud-lightning"),
            _ => ("Unknown", "cloud-question")
        };
    }
}

/// <summary>
///     Represents a snapshot of the current weather.
/// </summary>
/// <param name="TemperatureCelsius">The current temperature, in degrees Celsius.</param>
/// <param name="Description">A human-readable description of the current conditions.</param>
/// <param name="Icon">The bare Tabler icon name (without the <c>ti-</c> prefix) representing the current conditions.</param>
public sealed record WeatherSnapshot(double TemperatureCelsius, string Description, string Icon);
