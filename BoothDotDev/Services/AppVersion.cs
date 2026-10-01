using System.Reflection;

namespace BoothDotDev.Services;

/// <summary>
///     Provides the version of the running application.
/// </summary>
public static class AppVersion
{
    /// <summary>
    ///     Gets the full informational version, including the commit hash after a <c>+</c>.
    /// </summary>
    /// <value>The full version string, or <c>&lt;unknown&gt;</c> if the assembly doesn't carry one.</value>
    public static string Full { get; } =
        typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "<unknown>";

    /// <summary>
    ///     Gets <see cref="Full" /> with the commit hash cut to seven characters.
    /// </summary>
    /// <value>The shortened version string.</value>
    public static string Short { get; } = Shorten(Full);

    private static string Shorten(string version)
    {
        var hashIndex = version.IndexOf('+');
        return hashIndex >= 0 ? version[..Math.Min(version.Length, hashIndex + 8)] : version;
    }
}
