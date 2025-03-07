using BoothDotDev.Common.Data;
using BoothDotDev.Common.Data.Web;
using DEDrake;

namespace BoothDotDev.Data.Web;

/// <inheritdoc />
internal class DevChallenge : IDevChallenge
{
    /// <inheritdoc />
    public DateTimeOffset Date { get; internal set; }

    /// <inheritdoc />
    public string Description { get; internal set; } = string.Empty;

    /// <inheritdoc />
    public ShortGuid Id { get; internal set; }

    /// <inheritdoc />
    public int? OldId { get; internal set; }

    /// <inheritdoc />
    public string? Password { get; internal set; }

    /// <inheritdoc />
    public bool ShowSolution { get; internal set; }

    /// <inheritdoc />
    public string? Solution { get; internal set; }

    /// <inheritdoc />
    public string Title { get; internal set; } = string.Empty;

    /// <inheritdoc />
    public Visibility Visibility { get; internal set; }
}
