using BoothDotDev.Common.Data.Web;

namespace BoothDotDev.Data.Web;

/// <inheritdoc />
internal class DevChallenge : IDevChallenge
{
    /// <inheritdoc />
    public DateTimeOffset Date { get; internal set; }

    /// <inheritdoc />
    public string Description { get; internal set; } = string.Empty;

    /// <inheritdoc />
    public int Id { get; internal set; }

    /// <inheritdoc />
    public bool Published { get; internal set; }

    /// <inheritdoc />
    public bool ShowSolution { get; internal set; }

    /// <inheritdoc />
    public string? Solution { get; internal set; }

    /// <inheritdoc />
    public string Title { get; internal set; } = string.Empty;
}
