using System.ComponentModel.DataAnnotations;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Streaks;

/// <summary>
///     Represents the page model for editing a streak in the admin section.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Edit : PageModel
{
    private readonly MarkdownRenderingService _markdownRenderingService;
    private readonly StreakService _streakService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="streakService">The <see cref="StreakService" />.</param>
    /// <param name="markdownRenderingService">The Markdown rendering service.</param>
    public Edit(StreakService streakService, MarkdownRenderingService markdownRenderingService)
    {
        _streakService = streakService;
        _markdownRenderingService = markdownRenderingService;
    }

    /// <summary>
    ///     Gets or sets the streak being edited, if any.
    /// </summary>
    /// <value>The streak being edited, or default values if a new streak is being created.</value>
    [BindProperty]
    public EditModel Input { get; set; } = new();

    /// <summary>
    ///     Gets every logged check-in for this streak, newest first, if <see cref="EditModel.Mode" /> is
    ///     <see cref="StreakMode.CheckIn" />.
    /// </summary>
    /// <value>The streak's check-ins, newest first.</value>
    public IReadOnlyList<StreakCheckIn> CheckIns { get; private set; } = [];

    /// <summary>
    ///     Gets a value indicating whether a new streak is being created.
    /// </summary>
    /// <value><see langword="true" /> if a new streak is being created; otherwise, <see langword="false" />.</value>
    public bool CreatingNew { get; private set; }

    /// <summary>
    ///     Gets the ID of the draft that is currently live (published) for this streak.
    /// </summary>
    /// <value>The ID of the currently-live draft, or <see langword="null" /> if a new streak is being created.</value>
    public Guid? CurrentDraftId { get; private set; }

    /// <summary>
    ///     Gets the streak's full draft history, newest first, for the revision history panel.
    /// </summary>
    /// <value>The streak's drafts, ordered newest first.</value>
    public IReadOnlyList<StreakDraft> DraftHistory { get; private set; } = [];

    /// <summary>
    ///     Gets a value indicating whether the streak being edited is trashed.
    /// </summary>
    /// <value><see langword="true" /> if the streak is trashed; otherwise, <see langword="false" />.</value>
    public bool IsTrashed { get; private set; }

    /// <summary>
    ///     Gets every logged reset for this streak, newest first, if <see cref="EditModel.Mode" /> is
    ///     <see cref="StreakMode.CheckOut" />.
    /// </summary>
    /// <value>The streak's resets, newest first.</value>
    public IReadOnlyList<StreakReset> Resets { get; private set; } = [];

    /// <summary>
    ///     Gets the streak's computed current and best counts, if it has been saved.
    /// </summary>
    /// <value>The streak's stats, or <see langword="null" /> if a new streak is being created.</value>
    public StreakStats? Stats { get; private set; }

    /// <summary>
    ///     Gets the ID of the streak being edited.
    /// </summary>
    /// <value>The ID of the streak being edited, or <see langword="null" /> if a new streak is being created.</value>
    public Guid? StreakId { get; private set; }

    /// <summary>
    ///     Gets the ID of the draft currently loaded into the editor.
    /// </summary>
    /// <value>The ID of the draft being viewed, or <see langword="null" /> if a new streak is being created.</value>
    public Guid? ViewingDraftId { get; private set; }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="id">The ID of the streak to edit. If <see langword="null" />, a new streak will be created.</param>
    /// <param name="draftId">
    ///     The ID of a specific draft to view. If <see langword="null" />, the streak's newest draft is loaded - not
    ///     necessarily the currently-live one, so reopening the editor resumes from wherever editing was last left
    ///     off rather than silently discarding unpublished draft work.
    /// </param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(Guid? id, Guid? draftId)
    {
        if (!id.HasValue)
        {
            CreatingNew = true;
            Input = new EditModel { Visibility = Visibility.Published, Mode = StreakMode.CheckIn, CadenceUnit = StreakCadenceUnit.Day, CadenceInterval = 1 };
            return Page();
        }

        var streakResult = _streakService.GetStreakById(id.Value, true);
        if (streakResult.IsFailed)
        {
            return NotFound();
        }

        var draftResult = draftId.HasValue
            ? _streakService.GetDraft(id.Value, draftId.Value)
            : _streakService.GetNewestDraft(id.Value);

        if (draftResult.IsFailed)
        {
            return NotFound();
        }

        var streak = streakResult.Value;
        var draft = draftResult.Value;
        StreakId = streak.Id;
        CurrentDraftId = streak.CurrentDraftId;
        DraftHistory = _streakService.GetDraftHistory(id.Value);
        IsTrashed = streak.TrashedAt is not null;
        ViewingDraftId = draft.Id;
        Stats = _streakService.GetStats(streak);
        Input = new EditModel
        {
            Title = draft.Title,
            Body = draft.Body,
            Slug = streak.Slug,
            Mode = streak.Mode,
            StartedOn = streak.StartedOn,
            CadenceUnit = draft.CadenceUnit,
            CadenceInterval = draft.CadenceInterval,
            Color = draft.Color,
            DotColor = draft.DotColor,
            Visibility = draft.Visibility
        };

        if (streak.Mode == StreakMode.CheckIn)
        {
            CheckIns = [.. _streakService.GetCheckIns(id.Value).OrderByDescending(c => c.OccurredOn)];
        }
        else
        {
            Resets = [.. _streakService.GetResets(id.Value).OrderByDescending(r => r.OccurredOn)];
        }

        return Page();
    }

    /// <summary>
    ///     Handles the POST request for saving and publishing the streak, making it the streak's current draft.
    /// </summary>
    /// <param name="id">The ID of the streak being edited. If <see langword="null" />, a new streak is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSave(Guid? id)
    {
        CreatingNew = id is null;

        if (!ModelState.IsValid || !TryBuildSaveRequest(id, out var request))
        {
            return Page();
        }

        var result = id is null
            ? _streakService.CreateStreak(request)
            : _streakService.PublishStreak(id.Value, request);

        return RedirectOnSuccess(result);
    }

    /// <summary>
    ///     Handles the POST request for saving a draft of the streak, without publishing it. The streak's
    ///     currently-live draft, if any, is left unchanged.
    /// </summary>
    /// <param name="id">The ID of the streak being edited. If <see langword="null" />, a new streak is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSaveDraft(Guid? id)
    {
        CreatingNew = id is null;

        if (!ModelState.IsValid || !TryBuildSaveRequest(id, out var request))
        {
            return Page();
        }

        // A brand-new streak has no prior draft to leave untouched, so its first save - draft or not - always
        // becomes the streak's current draft. There's nothing else for it to sensibly point at.
        var result = id is null
            ? _streakService.CreateStreak(request)
            : _streakService.SaveDraft(id.Value, request);

        return RedirectOnSuccess(result);
    }

    /// <summary>
    ///     Handles the POST request for rendering a live preview of the streak's body.
    /// </summary>
    /// <param name="id">The ID of the streak being edited. If <see langword="null" />, a new streak is being created.</param>
    /// <returns>
    ///     A JSON payload of the rendered preview HTML. This handler backs the editor's live-updating preview pane
    ///     and is only ever called via <c>fetch</c> - there's no server-rendered fallback, since the Markdown editor
    ///     itself already requires JS to function.
    /// </returns>
    public IActionResult OnPostPreview(Guid? id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var html = _markdownRenderingService.Render(Input.Body, id ?? Guid.Empty, DateTimeOffset.UtcNow, "streaks");
        return new JsonResult(new { html, proseClass = "prose--sans" });
    }

    /// <summary>
    ///     Handles the POST request for moving the streak to the trash.
    /// </summary>
    /// <param name="id">The ID of the streak to trash.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid? id)
    {
        if (id is not { } streakId)
        {
            return BadRequest("Save the streak before it can be trashed.");
        }

        return RedirectOnSuccess(_streakService.TrashStreak(streakId));
    }

    /// <summary>
    ///     Handles the POST request for restoring the streak from the trash.
    /// </summary>
    /// <param name="id">The ID of the streak to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(Guid? id)
    {
        if (id is not { } streakId)
        {
            return BadRequest("Save the streak before it can be restored.");
        }

        return RedirectOnSuccess(_streakService.RestoreStreak(streakId));
    }

    /// <summary>
    ///     Handles the POST request for logging today as a completed check-in.
    /// </summary>
    /// <param name="id">The ID of the streak.</param>
    /// <returns>A redirect back to the editor.</returns>
    public IActionResult OnPostLogToday(Guid id)
    {
        _streakService.LogCheckIn(id, DateOnly.FromDateTime(DateTime.UtcNow));
        return RedirectToPage(new { id });
    }

    /// <summary>
    ///     Handles the POST request, from the calendar widget, for logging a specific day as either completed or
    ///     frozen.
    /// </summary>
    /// <param name="id">The ID of the streak.</param>
    /// <param name="date">The day being logged.</param>
    /// <param name="kind">Whether the day counts toward the streak's number.</param>
    /// <returns>A JSON payload of the streak's refreshed check-ins and stats.</returns>
    public IActionResult OnPostLogCheckIn(Guid id, DateOnly date, StreakCheckInKind kind)
    {
        _streakService.LogCheckIn(id, date, kind);
        return CalendarPayload(id);
    }

    /// <summary>
    ///     Handles the POST request, from the calendar widget, for removing a logged check-in day entirely.
    /// </summary>
    /// <param name="id">The ID of the streak.</param>
    /// <param name="date">The day to un-log.</param>
    /// <returns>A JSON payload of the streak's refreshed check-ins and stats.</returns>
    public IActionResult OnPostRemoveCheckIn(Guid id, DateOnly date)
    {
        _streakService.RemoveCheckIn(id, date);
        return CalendarPayload(id);
    }

    /// <summary>
    ///     Handles the POST request for backfilling a date range of check-ins, for importing a streak's existing
    ///     history in one go.
    /// </summary>
    /// <param name="id">The ID of the streak.</param>
    /// <param name="start">The first day to log, inclusive.</param>
    /// <param name="end">The last day to log, inclusive.</param>
    /// <param name="kind">Whether the backfilled days count toward the streak's number.</param>
    /// <returns>A redirect back to the editor.</returns>
    public IActionResult OnPostBackfill(Guid id, DateOnly start, DateOnly end, StreakCheckInKind kind)
    {
        var result = _streakService.BackfillCheckIns(id, start, end, kind);
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
        }

        return RedirectToPage(new { id });
    }

    /// <summary>
    ///     Handles the POST request, from the calendar widget, for removing every logged check-in within a
    ///     drag-selected date range in one go.
    /// </summary>
    /// <param name="id">The ID of the streak.</param>
    /// <param name="start">The first day to remove, inclusive.</param>
    /// <param name="end">The last day to remove, inclusive.</param>
    /// <returns>A JSON payload of the streak's refreshed check-ins and stats.</returns>
    public IActionResult OnPostBulkRemoveCheckIns(Guid id, DateOnly start, DateOnly end)
    {
        _streakService.BulkRemoveCheckIns(id, start, end);
        return CalendarPayload(id);
    }

    /// <summary>
    ///     Handles the POST request, from the calendar widget, for setting the kind of every already-logged check-in
    ///     within a drag-selected date range in one go.
    /// </summary>
    /// <param name="id">The ID of the streak.</param>
    /// <param name="start">The first day to edit, inclusive.</param>
    /// <param name="end">The last day to edit, inclusive.</param>
    /// <param name="kind">The kind to set every check-in in the range to.</param>
    /// <returns>A JSON payload of the streak's refreshed check-ins and stats.</returns>
    public IActionResult OnPostBulkSetCheckInKind(Guid id, DateOnly start, DateOnly end, StreakCheckInKind kind)
    {
        _streakService.BulkSetCheckInKind(id, start, end, kind);
        return CalendarPayload(id);
    }

    /// <summary>
    ///     Builds the JSON payload the calendar widget's <c>fetch</c> calls expect: the streak's full check-in log
    ///     and its freshly-recomputed stats, so the widget can redraw itself without a page reload.
    /// </summary>
    /// <param name="id">The ID of the streak.</param>
    /// <returns>A <see cref="JsonResult" />, or <see cref="NotFoundResult" /> if the streak doesn't exist.</returns>
    private IActionResult CalendarPayload(Guid id)
    {
        var streakResult = _streakService.GetStreakById(id, true);
        if (streakResult.IsFailed)
        {
            return NotFound();
        }

        var streak = streakResult.Value;
        var checkIns = _streakService.GetCheckIns(id).Select(c => new
        {
            date = c.OccurredOn.ToString("yyyy-MM-dd"),
            kind = c.Kind.ToString()
        });
        var stats = _streakService.GetStats(streak);

        return new JsonResult(new { checkIns, current = stats.Current, best = stats.Best });
    }

    /// <summary>
    ///     Handles the POST request for logging today as the day a check-out streak broke.
    /// </summary>
    /// <param name="id">The ID of the streak.</param>
    /// <returns>A redirect back to the editor.</returns>
    public IActionResult OnPostLogResetToday(Guid id)
    {
        _streakService.LogReset(id, DateOnly.FromDateTime(DateTime.UtcNow));
        return RedirectToPage(new { id });
    }

    /// <summary>
    ///     Handles the POST request for logging a specific day a check-out streak broke.
    /// </summary>
    /// <param name="id">The ID of the streak.</param>
    /// <param name="date">The day the streak broke.</param>
    /// <returns>A redirect back to the editor.</returns>
    public IActionResult OnPostLogReset(Guid id, DateOnly date)
    {
        _streakService.LogReset(id, date);
        return RedirectToPage(new { id });
    }

    /// <summary>
    ///     Handles the POST request for removing a logged reset.
    /// </summary>
    /// <param name="id">The ID of the streak.</param>
    /// <param name="date">The day to un-log.</param>
    /// <returns>A redirect back to the editor.</returns>
    public IActionResult OnPostRemoveReset(Guid id, DateOnly date)
    {
        _streakService.RemoveReset(id, date);
        return RedirectToPage(new { id });
    }

    /// <summary>
    ///     Handles the POST request for removing every logged reset within a date range in one go.
    /// </summary>
    /// <param name="id">The ID of the streak.</param>
    /// <param name="start">The first day to remove, inclusive.</param>
    /// <param name="end">The last day to remove, inclusive.</param>
    /// <returns>A redirect back to the editor.</returns>
    public IActionResult OnPostBulkRemoveResets(Guid id, DateOnly start, DateOnly end)
    {
        var result = _streakService.BulkRemoveResets(id, start, end);
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
        }

        return RedirectToPage(new { id });
    }

    /// <summary>
    ///     Builds a save request from the current state of <see cref="Input" />, validating the fields that only
    ///     apply to one of the two <see cref="StreakMode" /> values.
    /// </summary>
    /// <param name="id">The ID of the streak being edited, or <see langword="null" /> if a new streak is being created.</param>
    /// <param name="request">When this method returns, the built request, or <see langword="null" /> if invalid.</param>
    /// <returns><see langword="true" /> if the request is valid; otherwise, <see langword="false" />.</returns>
    private bool TryBuildSaveRequest(Guid? id, out StreakSaveRequest request)
    {
        request = null!;

        if (Input.Mode == StreakMode.CheckIn)
        {
            if (Input.CadenceUnit is null || Input.CadenceInterval is not { } interval || interval < 1)
            {
                ModelState.AddModelError(nameof(Input.CadenceInterval), "Enter a cadence unit and an interval of at least 1.");
                return false;
            }
        }
        else if (Input.StartedOn is null)
        {
            ModelState.AddModelError(nameof(Input.StartedOn), "Enter the date this streak started.");
            return false;
        }

        var sortOrder = id is { } streakId
            ? _streakService.GetStreakById(streakId, true).ValueOrDefault?.SortOrder ?? 0
            : _streakService.GetAllStreaks().Count;

        var cadenceUnit = Input.Mode == StreakMode.CheckIn ? Input.CadenceUnit : null;
        var cadenceInterval = Input.Mode == StreakMode.CheckIn ? Input.CadenceInterval : null;
        var startedOn = Input.Mode == StreakMode.CheckOut ? Input.StartedOn : null;

        var content = new StreakDraftContent(Input.Title, Input.Body, Input.Visibility, Input.Color, Input.DotColor, cadenceUnit,
            cadenceInterval);
        request = new StreakSaveRequest(Input.Slug, sortOrder, Input.Mode, startedOn, content);
        return true;
    }

    /// <summary>
    ///     Redirects back to this streak's edit page on success, or re-renders the form with an error on failure.
    /// </summary>
    /// <param name="result">The result of a save operation.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    private IActionResult RedirectOnSuccess(Result<Streak> result)
    {
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            return Page();
        }

        return RedirectToPage(new { id = result.Value.Id });
    }

    /// <summary>
    ///     Represents the model for editing a streak.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the title of the streak.
        /// </summary>
        /// <value>The title of the streak.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the body of the streak.
        /// </summary>
        /// <value>The body of the streak.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Body { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the slug of the streak, used as its anchor ID on the streaks page and in its own URL.
        /// </summary>
        /// <value>The slug of the streak.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets how the streak's current and best counts are derived.
        /// </summary>
        /// <value>The streak's tracking mode.</value>
        public StreakMode Mode { get; set; }

        /// <summary>
        ///     Gets or sets the date this streak started. Only used when <see cref="Mode" /> is
        ///     <see cref="StreakMode.CheckOut" />.
        /// </summary>
        /// <value>The start date.</value>
        public DateOnly? StartedOn { get; set; }

        /// <summary>
        ///     Gets or sets the cadence unit a check-in is expected every <see cref="CadenceInterval" /> of. Only used
        ///     when <see cref="Mode" /> is <see cref="StreakMode.CheckIn" />.
        /// </summary>
        /// <value>The cadence unit.</value>
        public StreakCadenceUnit? CadenceUnit { get; set; }

        /// <summary>
        ///     Gets or sets the number of <see cref="CadenceUnit" />s a check-in is expected within. Only used when
        ///     <see cref="Mode" /> is <see cref="StreakMode.CheckIn" />.
        /// </summary>
        /// <value>The cadence interval.</value>
        public int? CadenceInterval { get; set; }

        /// <summary>
        ///     Gets or sets the colour of the streak's card.
        /// </summary>
        /// <value>The colour of the streak, or <see langword="null" /> to derive it from the streak's position.</value>
        public PaletteHue? Color { get; set; }

        /// <summary>
        ///     Gets or sets the colour of the streak's dots.
        /// </summary>
        /// <value>The colour of the dots, or <see langword="null" /> to fall back to <see cref="Color" />.</value>
        public PaletteHue? DotColor { get; set; }

        /// <summary>
        ///     Gets or sets the visibility of the streak.
        /// </summary>
        /// <value>The visibility of the streak.</value>
        public Visibility Visibility { get; set; } = Visibility.Published;
    }
}
