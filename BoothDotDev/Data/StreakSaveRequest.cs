namespace BoothDotDev.Data;

/// <summary>
///     Represents the content of a streak draft snapshot.
/// </summary>
/// <param name="Title">The title of the streak.</param>
/// <param name="Body">The body of the streak.</param>
/// <param name="Visibility">The visibility of the streak.</param>
/// <param name="Color">The colour of the streak's card, or <see langword="null" /> to derive it from its position.</param>
/// <param name="DotColor">The colour of the streak's dots, or <see langword="null" /> to fall back to <paramref name="Color" />.</param>
/// <param name="CadenceUnit">The cadence unit, or <see langword="null" /> if the streak's mode is <see cref="StreakMode.CheckOut" />.</param>
/// <param name="CadenceInterval">
///     The number of <paramref name="CadenceUnit" />s a check-in is expected within, or <see langword="null" /> if the
///     streak's mode is <see cref="StreakMode.CheckOut" />.
/// </param>
public sealed record StreakDraftContent(
    string Title,
    string Body,
    Visibility Visibility,
    PaletteHue? Color,
    PaletteHue? DotColor,
    StreakCadenceUnit? CadenceUnit,
    int? CadenceInterval);

/// <summary>
///     Represents a request to create or save a streak, bundling its parent-level fields with the content of the
///     draft the save produces.
/// </summary>
/// <param name="Slug">The slug of the streak.</param>
/// <param name="SortOrder">The streak's position on the streaks page.</param>
/// <param name="Mode">How the streak's current and best counts are derived.</param>
/// <param name="StartedOn">The date the streak started, or <see langword="null" /> if <paramref name="Mode" /> is <see cref="StreakMode.CheckIn" />.</param>
/// <param name="Content">The content of the draft this save produces.</param>
public sealed record StreakSaveRequest(string Slug, int SortOrder, StreakMode Mode, DateOnly? StartedOn, StreakDraftContent Content);
