using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Humanizer;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages;

/// <summary>
///     Represents the model for the streaks page.
/// </summary>
public sealed class Streaks : PageModel
{
    private const int MaxDots = 28;

    private readonly StreakService _streakService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Streaks" /> class.
    /// </summary>
    /// <param name="streakService">The <see cref="StreakService" />.</param>
    public Streaks(StreakService streakService)
    {
        _streakService = streakService;
    }

    /// <summary>
    ///     Gets every published, non-trashed streak, in curated display order.
    /// </summary>
    /// <value>The streaks to render on the page.</value>
    public IReadOnlyList<Streak> AllStreaks { get; private set; } = [];

    /// <summary>
    ///     Gets the date and time the page was last updated, i.e. the most recent update across every streak.
    /// </summary>
    /// <value>The most recent update date and time, or <see langword="null" /> if there are no streaks yet.</value>
    public DateTimeOffset? UpdatedAt
    {
        get => AllStreaks.Count == 0 ? null : AllStreaks.Max(s => s.UpdatedAt ?? s.PublishedAt);
    }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        AllStreaks = _streakService.GetPublishedStreaks();
    }

    /// <summary>
    ///     Gets a streak's computed current and best counts.
    /// </summary>
    /// <param name="streak">The streak.</param>
    /// <returns>The streak's stats.</returns>
    public StreakStats GetStats(Streak streak)
    {
        return _streakService.GetStats(streak);
    }

    /// <summary>
    ///     Describes a streak's cadence for its badge, e.g. "daily", "every 2 weeks", "ongoing".
    /// </summary>
    /// <param name="streak">The streak.</param>
    /// <returns>The cadence label.</returns>
    public static string CadenceLabel(Streak streak)
    {
        if (streak.Mode == StreakMode.CheckOut)
        {
            return "ongoing";
        }

        var unit = streak.CadenceUnit!.Value;
        var interval = streak.CadenceInterval!.Value;

        return (unit, interval) switch
        {
            (StreakCadenceUnit.Day, 1) => "daily",
            (StreakCadenceUnit.Week, 1) => "weekly",
            (StreakCadenceUnit.Month, 1) => "monthly",
            (StreakCadenceUnit.Month, 3) => "quarterly",
            (StreakCadenceUnit.Year, 1) => "annually",
            _ => $"every {interval} {unit.ToString().ToLowerInvariant().ToQuantity(interval, ShowQuantityAs.None)}"
        };
    }

    /// <summary>
    ///     Describes the unit a streak's count is measured in, e.g. "days", "weeks".
    /// </summary>
    /// <param name="streak">The streak.</param>
    /// <param name="count">The count being labelled, so the unit is correctly singular or plural.</param>
    /// <returns>The unit label.</returns>
    public static string UnitLabel(Streak streak, int count)
    {
        var unit = streak.Mode == StreakMode.CheckOut ? StreakCadenceUnit.Day : streak.CadenceUnit!.Value;
        return unit.ToString().ToLowerInvariant().ToQuantity(count, ShowQuantityAs.None);
    }

    /// <summary>
    ///     Gets the dots to render for a streak's display row, oldest first.
    /// </summary>
    /// <param name="streak">The streak.</param>
    /// <param name="stats">The streak's computed stats.</param>
    /// <returns>
    ///     One entry per dot; <see langword="true" /> for a frozen day. For a check-in streak this is the tail of its
    ///     actual current run, so a frozen day shows up as such; a check-out streak has no per-day log to draw from,
    ///     so it's an abstract count of filled (never frozen) dots instead.
    /// </returns>
    public IReadOnlyList<bool> GetDots(Streak streak, StreakStats stats)
    {
        if (streak.Mode == StreakMode.CheckOut)
        {
            return new bool[Math.Min(stats.Current, MaxDots)];
        }

        return
        [
            .. _streakService.GetCurrentRunCheckIns(streak)
                .TakeLast(MaxDots)
                .Select(c => c.Kind == StreakCheckInKind.Frozen)
        ];
    }
}
