using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages;

/// <summary>
///     Represents the model for a single streak's detail page - its full logged history, laid out differently
///     depending on its cadence (a daily dot grid, a per-period dot grid, or a timeline).
/// </summary>
public sealed class StreakDetail : PageModel
{
    private const int TimelinePageSize = 10;
    private const int YearsPerPage = 10;

    private readonly StreakService _streakService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StreakDetail" /> class.
    /// </summary>
    /// <param name="streakService">The <see cref="StreakService" />.</param>
    public StreakDetail(StreakService streakService)
    {
        _streakService = streakService;
    }

    /// <summary>
    ///     Gets every published, non-trashed streak, for the switcher pills at the top of the page.
    /// </summary>
    /// <value>The published streaks.</value>
    public IReadOnlyList<Streak> AllStreaks { get; private set; } = [];

    /// <summary>
    ///     Gets a value indicating whether there's an older page to go to.
    /// </summary>
    /// <value><see langword="true" /> unless the timeline view has run out of history.</value>
    public bool CanGoOlder { get; private set; } = true;

    /// <summary>
    ///     Gets a value indicating whether there's a newer page to go back to.
    /// </summary>
    /// <value><see langword="true" /> if <see cref="Offset" /> is greater than zero.</value>
    public bool CanGoNewer
    {
        get => Offset > 0;
    }

    /// <summary>
    ///     Gets the number of blank cells to render before the first day of the month, for <see cref="StreakViewKind.DailyDots" />.
    /// </summary>
    /// <value>The number of leading blank cells.</value>
    public int DailyLeadingBlanks { get; private set; }

    /// <summary>
    ///     Gets the how-far-back page being viewed. Zero is the most recent page; each unit further back means one
    ///     month, one year, one decade, or one timeline page, depending on <see cref="ViewKind" />.
    /// </summary>
    /// <value>The page offset.</value>
    public int Offset { get; private set; }

    /// <summary>
    ///     Gets the label for the page currently being viewed, e.g. "September 2026", "year 2026", "2017–2026".
    /// </summary>
    /// <value>The page label.</value>
    public string PageLabel { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the cells to render for <see cref="StreakViewKind.DailyDots" />, one per day of the month being viewed.
    /// </summary>
    /// <value>The daily cells.</value>
    public IReadOnlyList<StreakDetailCell> DailyCells { get; private set; } = [];

    /// <summary>
    ///     Gets the cells to render for <see cref="StreakViewKind.PeriodDots" />, one per period on the page being viewed.
    /// </summary>
    /// <value>The period cells.</value>
    public IReadOnlyList<StreakDetailCell> PeriodCells { get; private set; } = [];

    /// <summary>
    ///     Gets how many <see cref="PeriodCells" /> to lay out per row.
    /// </summary>
    /// <value>The number of columns.</value>
    public int PeriodColumns { get; private set; } = 6;

    /// <summary>
    ///     Gets the items to render for <see cref="StreakViewKind.Timeline" />, newest first.
    /// </summary>
    /// <value>The timeline items.</value>
    public IReadOnlyList<StreakDetailCell> TimelineItems { get; private set; } = [];

    /// <summary>
    ///     Gets the streak this page is showing.
    /// </summary>
    /// <value>The streak.</value>
    public Streak Streak { get; private set; } = null!;

    /// <summary>
    ///     Gets the streak's computed current and best counts.
    /// </summary>
    /// <value>The streak's stats.</value>
    public StreakStats Stats { get; private set; } = null!;

    /// <summary>
    ///     Gets which layout this streak's cadence uses.
    /// </summary>
    /// <value>The view kind.</value>
    public StreakViewKind ViewKind { get; private set; }

    /// <summary>
    ///     Gets the weekday header labels for <see cref="StreakViewKind.DailyDots" />, Monday first.
    /// </summary>
    /// <value>The weekday labels.</value>
    public IReadOnlyList<string> WeekdayLabels { get; } = ["mo", "tu", "we", "th", "fr", "sa", "su"];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="slug">The slug of the streak to show.</param>
    /// <param name="offset">How many pages back to view. Zero shows the most recent page.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(string slug, int offset = 0)
    {
        var result = _streakService.GetStreakBySlug(slug);
        if (result.IsFailed || result.Value.Visibility != Visibility.Published)
        {
            return NotFound();
        }

        Streak = result.Value;
        Stats = _streakService.GetStats(Streak);
        Offset = Math.Max(0, offset);
        AllStreaks = _streakService.GetPublishedStreaks();
        ViewKind = DetermineViewKind(Streak);

        switch (ViewKind)
        {
            case StreakViewKind.DailyDots:
                BuildDaily();
                break;
            case StreakViewKind.PeriodDots:
                BuildPeriods();
                break;
            default:
                BuildTimeline();
                break;
        }

        return Page();
    }

    private static StreakViewKind DetermineViewKind(Streak streak)
    {
        if (streak.Mode == StreakMode.CheckOut)
        {
            return StreakViewKind.DailyDots;
        }

        var unit = streak.CadenceUnit!.Value;
        var interval = streak.CadenceInterval!.Value;

        return (unit, interval) switch
        {
            (StreakCadenceUnit.Day, 1) => StreakViewKind.DailyDots,
            (StreakCadenceUnit.Day, _) => StreakViewKind.Timeline,
            (StreakCadenceUnit.Year, 1) => StreakViewKind.PeriodDots,
            (StreakCadenceUnit.Year, _) => StreakViewKind.Timeline,
            _ => StreakViewKind.PeriodDots // Week or Month, any interval
        };
    }

    /// <summary>
    ///     Builds a month grid of days for a daily check-in streak or a check-out streak.
    /// </summary>
    private void BuildDaily()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monthStart = new DateOnly(today.Year, today.Month, 1).AddMonths(-Offset);
        var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
        DailyLeadingBlanks = ((int)new DateOnly(monthStart.Year, monthStart.Month, 1).DayOfWeek + 6) % 7;
        PageLabel = monthStart.ToDateTime(TimeOnly.MinValue).ToString("MMMM yyyy");

        DateOnly? rangeStart;
        Dictionary<DateOnly, StreakCheckInKind>? checkInsByDate = null;
        HashSet<DateOnly>? resetDates = null;

        if (Streak.Mode == StreakMode.CheckIn)
        {
            var checkIns = _streakService.GetCheckIns(Streak.Id);
            rangeStart = checkIns.Count > 0 ? checkIns[0].OccurredOn : null;
            checkInsByDate = checkIns.ToDictionary(c => c.OccurredOn, c => c.Kind);
        }
        else
        {
            rangeStart = Streak.StartedOn;
            resetDates = [.. _streakService.GetResets(Streak.Id).Select(r => r.OccurredOn)];
        }

        var cells = new List<StreakDetailCell>(daysInMonth);
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(monthStart.Year, monthStart.Month, day);
            var isToday = date == today;
            StreakDayState state;

            if (date > today || rangeStart is null || date < rangeStart)
            {
                state = StreakDayState.Blank;
            }
            else if (Streak.Mode == StreakMode.CheckIn)
            {
                state = checkInsByDate!.TryGetValue(date, out var kind)
                    ? kind == StreakCheckInKind.Frozen ? StreakDayState.Frozen : StreakDayState.Completed
                    : StreakDayState.Missed;
            }
            else
            {
                state = resetDates!.Contains(date) ? StreakDayState.Reset : StreakDayState.Completed;
            }

            cells.Add(new StreakDetailCell(day.ToString(), state, isToday));
        }

        DailyCells = cells;
    }

    /// <summary>
    ///     Builds a dot per calendar period (week, month, quarter, or year) for the page being viewed.
    /// </summary>
    private void BuildPeriods()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var checkIns = _streakService.GetCheckIns(Streak.Id);
        var unit = Streak.CadenceUnit!.Value;

        if (unit == StreakCadenceUnit.Year)
        {
            BuildYearPeriods(checkIns, today);
            return;
        }

        var interval = Streak.CadenceInterval!.Value;
        var periodsPerYear = unit == StreakCadenceUnit.Week
            ? (int)Math.Ceiling(365.0 / (interval * 7))
            : (int)Math.Ceiling(12.0 / interval);

        PeriodColumns = unit == StreakCadenceUnit.Week ? 13 : interval == 3 ? 4 : 6;

        var year = today.Year - Offset;
        PageLabel = $"year {year}";

        var cells = new List<StreakDetailCell>(periodsPerYear);
        for (var index = 0; index < periodsPerYear; index++)
        {
            var (start, end, label) = unit == StreakCadenceUnit.Week
                ? WeekPeriodBounds(year, index, interval)
                : MonthPeriodBounds(year, index, interval);

            cells.Add(BuildPeriodCell(checkIns, start, end, label, today));
        }

        PeriodCells = cells;
    }

    private void BuildYearPeriods(IReadOnlyList<StreakCheckIn> checkIns, DateOnly today)
    {
        PeriodColumns = 5;
        var endYear = today.Year - Offset * YearsPerPage;
        var startYear = endYear - YearsPerPage + 1;
        PageLabel = $"{startYear}–{endYear}";

        var cells = new List<StreakDetailCell>(YearsPerPage);
        for (var year = startYear; year <= endYear; year++)
        {
            cells.Add(BuildPeriodCell(checkIns, new DateOnly(year, 1, 1), new DateOnly(year, 12, 31), year.ToString(), today));
        }

        PeriodCells = cells;
    }

    private static (DateOnly Start, DateOnly End, string Label) WeekPeriodBounds(int year, int index, int interval)
    {
        var start = new DateOnly(year, 1, 1).AddDays(index * interval * 7);
        var end = start.AddDays(interval * 7 - 1);
        var label = interval == 1 ? $"w{index + 1}" : $"p{index + 1}";
        return (start, end, label);
    }

    private static (DateOnly Start, DateOnly End, string Label) MonthPeriodBounds(int year, int index, int interval)
    {
        var startMonth = index * interval + 1;
        var start = new DateOnly(year, startMonth, 1);
        var endMonth = Math.Min(startMonth + interval - 1, 12);
        var end = new DateOnly(year, endMonth, DateTime.DaysInMonth(year, endMonth));

        var label = interval switch
        {
            1 => start.ToString("MMM"),
            3 => $"Q{index + 1}",
            _ => $"p{index + 1}"
        };

        return (start, end, label);
    }

    /// <summary>
    ///     Builds one period's cell from whichever check-ins fall within it: completed beats frozen beats a plain
    ///     miss, and a period that hasn't ended yet is left blank rather than marked as missed.
    /// </summary>
    private static StreakDetailCell BuildPeriodCell(IReadOnlyList<StreakCheckIn> checkIns, DateOnly start, DateOnly end, string label,
        DateOnly today)
    {
        if (start > today)
        {
            return new StreakDetailCell(label, StreakDayState.Blank, false);
        }

        var isToday = today >= start && today <= end;
        var inPeriod = checkIns.Where(c => c.OccurredOn >= start && c.OccurredOn <= end);

        var hasCompleted = false;
        var hasFrozen = false;
        foreach (var checkIn in inPeriod)
        {
            if (checkIn.Kind == StreakCheckInKind.Completed)
            {
                hasCompleted = true;
            }
            else
            {
                hasFrozen = true;
            }
        }

        var state = hasCompleted
            ? StreakDayState.Completed
            : hasFrozen
                ? StreakDayState.Frozen
                : end < today
                    ? StreakDayState.Missed
                    : StreakDayState.Blank;

        return new StreakDetailCell(label, state, isToday);
    }

    /// <summary>
    ///     Builds a page of the most recent logged days, newest first, for a cadence too sparse or irregular for a
    ///     dot grid to read naturally.
    /// </summary>
    private void BuildTimeline()
    {
        var unit = Streak.CadenceUnit!.Value;
        var interval = Streak.CadenceInterval!.Value;
        PageLabel = $"every {interval} {(unit == StreakCadenceUnit.Day ? "days" : "years")}";

        var checkIns = _streakService.GetCheckIns(Streak.Id);
        if (checkIns.Count == 0)
        {
            TimelineItems = [];
            CanGoOlder = false;
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var newestFirst = checkIns.Reverse().ToList();
        var page = newestFirst.Skip(Offset * TimelinePageSize).Take(TimelinePageSize);

        TimelineItems =
        [
            .. page.Select(c => new StreakDetailCell(
                c.OccurredOn.ToString("d MMM yyyy"),
                c.Kind == StreakCheckInKind.Frozen ? StreakDayState.Frozen : StreakDayState.Completed,
                c.OccurredOn == today))
        ];

        CanGoOlder = newestFirst.Count > (Offset + 1) * TimelinePageSize;
    }
}
