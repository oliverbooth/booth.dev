using System.Reflection;
using System.Text.Json;

namespace BoothDotDev.Vite;

internal static class ViteManifest
{
    /// <summary>
    ///     The base URL for the Vite development server, used for hot module replacement (HMR) in development mode.
    /// </summary>
    public const string BaseViteHmrUrl = "http://localhost:5173";

    private static Dictionary<string, ManifestEntry>? _manifest;

    /// <summary>
    ///     Resolves the given source path to the corresponding file path in the Vite manifest.
    /// </summary>
    /// <param name="sourcePath">The source path to resolve.</param>
    /// <param name="environment">The web host environment.</param>
    /// <returns>The resolved file path.</returns>
    /// <exception cref="InvalidOperationException">No manifest entry is found for the given source path.</exception>
    public static string Resolve(string sourcePath, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            return $"{BaseViteHmrUrl}/{sourcePath}";
        }

        _manifest ??= LoadManifest();
        return _manifest.TryGetValue(sourcePath, out var entry)
            ? $"/{entry.File}"
            : throw new InvalidOperationException($"No manifest entry for {sourcePath}");
    }

    private static Dictionary<string, ManifestEntry> LoadManifest()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames().Single(n => n.EndsWith("vite.manifest.json"));

        using var stream = assembly.GetManifestResourceStream(resourceName)
                           ?? throw new InvalidOperationException("Vite manifest embedded resource not found.");

        return JsonSerializer.Deserialize<Dictionary<string, ManifestEntry>>(stream)
               ?? throw new InvalidOperationException("Failed to deserialize Vite manifest.");
    }
}
