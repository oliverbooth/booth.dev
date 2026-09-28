using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for managing streaks and their logged history.
/// </summary>
public sealed class StreakService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StreakService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="IDbContextFactory{TContext}" />.</param>
    public StreakService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Creates a new streak, along with its first draft, which immediately becomes the streak's current draft.
    ///     The streak is appended to the end of the current sort order.
    /// </summary>
    /// <param name="request">The streak's parent-level fields and the content of its first draft.</param>
    /// <returns>A <see cref="Result{T}" /> containing the newly-created streak.</returns>
    public Result<Streak> CreateStreak(StreakSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();

        if (SlugInUse(context, request.Slug, null))
        {
            return Result.Fail($"Slug '{request.Slug}' is already in use.");
        }

        var streak = new Streak { Slug = request.Slug, SortOrder = request.SortOrder, Mode = request.Mode, StartedOn = request.StartedOn };

        // two SaveChanges calls, not one: Streak -> StreakDraft (via StreakId) and StreakDraft -> Streak (via
        // CurrentDraftId) form a cycle between two rows that are both new, which EF can't resolve in a single call
        // even though CurrentDraftId is nullable.
        context.Streaks.Add(streak);
        context.SaveChanges();

        var draft = NewDraft(streak.Id, request.Content);
        context.StreakDrafts.Add(draft);
        streak.CurrentDraftId = draft.Id;
        context.SaveChanges();

        return streak;
    }

    /// <summary>
    ///     Saves a new draft of an existing streak, without publishing it. The streak's currently-live draft, if
    ///     any, is left unchanged, so the public page is unaffected.
    /// </summary>
    /// <param name="id">The ID of the streak to save a draft for.</param>
    /// <param name="request">The streak's parent-level fields and the content of the new draft.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the streak the draft was saved for, or an error if no streak with
    ///     the specified <paramref name="id" /> exists.
    /// </returns>
    public Result<Streak> SaveDraft(Guid id, StreakSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var streak = context.Streaks.Find(id);

        if (streak is null)
        {
            return Result.Fail($"Streak with ID '{id}' not found.");
        }

        if (SlugInUse(context, request.Slug, id))
        {
            return Result.Fail($"Slug '{request.Slug}' is already in use.");
        }

        var draft = NewDraft(streak.Id, request.Content);
        context.StreakDrafts.Add(draft);

        streak.Slug = request.Slug;
        streak.SortOrder = request.SortOrder;
        streak.Mode = request.Mode;
        streak.StartedOn = request.StartedOn;

        context.SaveChanges();
        return streak;
    }

    /// <summary>
    ///     Saves a new draft of an existing streak and publishes it, making it the streak's current draft.
    /// </summary>
    /// <param name="id">The ID of the streak to publish.</param>
    /// <param name="request">The streak's parent-level fields and the content of the new draft.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the updated streak, or an error if no streak with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<Streak> PublishStreak(Guid id, StreakSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var streak = context.Streaks.Find(id);

        if (streak is null)
        {
            return Result.Fail($"Streak with ID '{id}' not found.");
        }

        if (SlugInUse(context, request.Slug, id))
        {
            return Result.Fail($"Slug '{request.Slug}' is already in use.");
        }

        var draft = NewDraft(streak.Id, request.Content);
        context.StreakDrafts.Add(draft);

        streak.Slug = request.Slug;
        streak.SortOrder = request.SortOrder;
        streak.Mode = request.Mode;
        streak.StartedOn = request.StartedOn;
        streak.CurrentDraftId = draft.Id;
        streak.UpdatedAt = DateTimeOffset.UtcNow;

        context.SaveChanges();
        return streak;
    }

    /// <summary>
    ///     Moves a streak to the trash. It's excluded from the public page, but nothing about it is otherwise
    ///     touched, and it can be restored with <see cref="RestoreStreak" />.
    /// </summary>
    /// <param name="id">The ID of the streak to trash.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the trashed streak, or an error if no streak with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<Streak> TrashStreak(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var streak = context.Streaks.Find(id);

        if (streak is null)
        {
            return Result.Fail($"The streak with ID {id} was not found");
        }

        streak.TrashedAt = DateTimeOffset.UtcNow;
        context.SaveChanges();
        return streak;
    }

    /// <summary>
    ///     Restores a trashed streak, making it visible on the public page again.
    /// </summary>
    /// <param name="id">The ID of the streak to restore.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the restored streak, or an error if no streak with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<Streak> RestoreStreak(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var streak = context.Streaks.Find(id);

        if (streak is null)
        {
            return Result.Fail($"The streak with ID {id} was not found");
        }

        streak.TrashedAt = null;
        context.SaveChanges();
        return streak;
    }

    /// <summary>
    ///     Permanently deletes a trashed streak - the streak row, every draft in its revision history, and every
    ///     logged check-in or reset (all cascade). This cannot be undone.
    /// </summary>
    /// <param name="id">The ID of the streak to permanently delete.</param>
    /// <returns>
    ///     A <see cref="Result" /> indicating success, or a failure if no streak with the specified
    ///     <paramref name="id" /> exists or it isn't currently trashed.
    /// </returns>
    public Result PermanentlyDeleteStreak(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var streak = context.Streaks.Find(id);

        if (streak is null)
        {
            return Result.Fail($"The streak with ID {id} was not found");
        }

        if (streak.TrashedAt is null)
        {
            return Result.Fail("Only trashed streaks can be permanently deleted.");
        }

        context.Streaks.Remove(streak);
        context.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Gets every streak, in curated display order, for the admin listing.
    /// </summary>
    /// <returns>A read-only view of every streak in <see cref="Streak.SortOrder" />, excluding trashed ones.</returns>
    public IReadOnlyList<Streak> GetAllStreaks()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return [.. context.Streaks.Include(s => s.CurrentDraft).Where(s => s.TrashedAt == null).OrderBy(s => s.SortOrder)];
    }

    /// <summary>
    ///     Gets every published, non-trashed streak, in curated display order, for the public page.
    /// </summary>
    /// <returns>A read-only view of every published streak, in <see cref="Streak.SortOrder" />.</returns>
    public IReadOnlyList<Streak> GetPublishedStreaks()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return
        [
            .. context.Streaks.Include(s => s.CurrentDraft)
                .Where(s => s.TrashedAt == null && s.CurrentDraft!.Visibility == Visibility.Published)
                .OrderBy(s => s.SortOrder)
        ];
    }

    /// <summary>
    ///     Gets a streak by its ID.
    /// </summary>
    /// <param name="id">The ID of the streak.</param>
    /// <param name="includeTrashed">
    ///     Whether to include the streak if it's trashed. Only the admin editor should pass <see langword="true" /> -
    ///     every public-facing caller should get the trash exclusion for free.
    /// </param>
    /// <returns>A <see cref="Result{T}" /> containing the streak if found; otherwise, an error result.</returns>
    public Result<Streak> GetStreakById(Guid id, bool includeTrashed = false)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var streak = context.Streaks.Include(s => s.CurrentDraft).FirstOrDefault(s => s.Id == id);

        if (streak is null || (streak.TrashedAt is not null && !includeTrashed))
        {
            return Result.Fail($"The streak with ID {id} was not found");
        }

        return streak;
    }

    /// <summary>
    ///     Gets a streak by its slug.
    /// </summary>
    /// <param name="slug">The slug of the streak.</param>
    /// <param name="includeTrashed">
    ///     Whether to include the streak if it's trashed. Only the admin editor should pass <see langword="true" /> -
    ///     every public-facing caller should get the trash exclusion for free.
    /// </param>
    /// <returns>A <see cref="Result{T}" /> containing the streak if found; otherwise, an error result.</returns>
    public Result<Streak> GetStreakBySlug(string slug, bool includeTrashed = false)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var streak = context.Streaks.Include(s => s.CurrentDraft).FirstOrDefault(s => s.Slug == slug);

        if (streak is null || (streak.TrashedAt is not null && !includeTrashed))
        {
            return Result.Fail($"The streak with slug '{slug}' was not found");
        }

        return streak;
    }

    /// <summary>
    ///     Gets all trashed streaks, newest-trashed first.
    /// </summary>
    /// <returns>A read-only view of all trashed streaks.</returns>
    public IReadOnlyList<Streak> GetTrashedStreaks()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return [.. context.Streaks.Include(s => s.CurrentDraft).Where(s => s.TrashedAt != null).OrderByDescending(s => s.TrashedAt)];
    }

    /// <summary>
    ///     Returns a streak's full draft history, newest first.
    /// </summary>
    /// <param name="id">The ID of the streak whose draft history to return.</param>
    /// <returns>The streak's drafts, newest first.</returns>
    public IReadOnlyList<StreakDraft> GetDraftHistory(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return [.. context.StreakDrafts.Where(d => d.StreakId == id).OrderByDescending(d => d.CreatedAt)];
    }

    /// <summary>
    ///     Returns a specific draft of the specified streak, for viewing without publishing it.
    /// </summary>
    /// <param name="id">The ID of the streak the draft belongs to.</param>
    /// <param name="draftId">The ID of the draft to return.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the requested draft, or an error if it doesn't exist or doesn't
    ///     belong to the specified streak.
    /// </returns>
    public Result<StreakDraft> GetDraft(Guid id, Guid draftId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var draft = context.StreakDrafts.Find(draftId);

        if (draft is null || draft.StreakId != id)
        {
            return Result.Fail($"Draft '{draftId}' not found for streak '{id}'.");
        }

        return draft;
    }

    /// <summary>
    ///     Returns the newest draft of the specified streak, which may or may not be the streak's current
    ///     (published) draft.
    /// </summary>
    /// <param name="id">The ID of the streak whose newest draft to return.</param>
    /// <returns>A <see cref="Result{T}" /> containing the streak's newest draft, or an error if it has no drafts.</returns>
    public Result<StreakDraft> GetNewestDraft(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var draft = context.StreakDrafts.Where(d => d.StreakId == id).OrderByDescending(d => d.CreatedAt).FirstOrDefault();

        if (draft is null)
        {
            return Result.Fail($"Streak '{id}' has no drafts.");
        }

        return draft;
    }

    /// <summary>
    ///     Reassigns every streak's <see cref="Streak.SortOrder" /> to match its position in
    ///     <paramref name="orderedIds" />, for the admin drag-to-reorder list.
    /// </summary>
    /// <param name="orderedIds">Every non-trashed streak's ID, in its new display order.</param>
    /// <returns>A <see cref="Result" /> indicating success.</returns>
    public Result Reorder(IReadOnlyList<Guid> orderedIds)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var streaks = context.Streaks.Where(s => orderedIds.Contains(s.Id)).ToDictionary(s => s.Id);

        for (var i = 0; i < orderedIds.Count; i++)
        {
            if (streaks.TryGetValue(orderedIds[i], out var streak))
            {
                streak.SortOrder = i;
            }
        }

        context.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Gets every logged check-in for a check-in streak, oldest first.
    /// </summary>
    /// <param name="streakId">The ID of the streak.</param>
    /// <returns>The streak's check-ins, oldest first.</returns>
    public IReadOnlyList<StreakCheckIn> GetCheckIns(Guid streakId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return [.. context.StreakCheckIns.Where(c => c.StreakId == streakId).OrderBy(c => c.OccurredOn)];
    }

    /// <summary>
    ///     Gets every logged reset for a check-out streak, oldest first.
    /// </summary>
    /// <param name="streakId">The ID of the streak.</param>
    /// <returns>The streak's resets, oldest first.</returns>
    public IReadOnlyList<StreakReset> GetResets(Guid streakId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return [.. context.StreakResets.Where(r => r.StreakId == streakId).OrderBy(r => r.OccurredOn)];
    }

    /// <summary>
    ///     Logs a single day of a check-in streak, either as done or as excused (frozen). Logging over an
    ///     already-logged day replaces it.
    /// </summary>
    /// <param name="streakId">The ID of the streak to log against.</param>
    /// <param name="occurredOn">The day being logged.</param>
    /// <param name="kind">Whether the day counts toward the streak's number.</param>
    /// <param name="note">A short note about the day, if any.</param>
    /// <returns>A <see cref="Result" /> indicating success.</returns>
    public Result LogCheckIn(Guid streakId, DateOnly occurredOn, StreakCheckInKind kind = StreakCheckInKind.Completed, string? note = null)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var existing = context.StreakCheckIns.FirstOrDefault(c => c.StreakId == streakId && c.OccurredOn == occurredOn);

        if (existing is null)
        {
            context.StreakCheckIns.Add(new StreakCheckIn { StreakId = streakId, OccurredOn = occurredOn, Kind = kind, Note = note });
        }
        else
        {
            existing.Kind = kind;
            existing.Note = note;
        }

        context.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Removes a logged check-in day entirely, e.g. to correct a mistaken backfill.
    /// </summary>
    /// <param name="streakId">The ID of the streak to remove the check-in from.</param>
    /// <param name="occurredOn">The day to un-log.</param>
    /// <returns>A <see cref="Result" /> indicating success, or a failure if no check-in exists for that day.</returns>
    public Result RemoveCheckIn(Guid streakId, DateOnly occurredOn)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var existing = context.StreakCheckIns.FirstOrDefault(c => c.StreakId == streakId && c.OccurredOn == occurredOn);

        if (existing is null)
        {
            return Result.Fail("No check-in exists for that day.");
        }

        context.StreakCheckIns.Remove(existing);
        context.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Logs every day in a date range as a check-in, for importing a streak's existing history in one go. Days
    ///     already logged within the range are left untouched, so this never overwrites a day that's already been
    ///     marked frozen or given a note.
    /// </summary>
    /// <param name="streakId">The ID of the streak to backfill.</param>
    /// <param name="start">The first day to log, inclusive.</param>
    /// <param name="end">The last day to log, inclusive.</param>
    /// <param name="kind">Whether the backfilled days count toward the streak's number.</param>
    /// <returns>A <see cref="Result" /> indicating success, or a failure if the range is invalid.</returns>
    public Result BackfillCheckIns(Guid streakId, DateOnly start, DateOnly end, StreakCheckInKind kind = StreakCheckInKind.Completed)
    {
        if (end < start)
        {
            return Result.Fail("The end date must be on or after the start date.");
        }

        using var context = _dbContextFactory.CreateDbContext();
        var existingDates = context.StreakCheckIns
            .Where(c => c.StreakId == streakId && c.OccurredOn >= start && c.OccurredOn <= end)
            .Select(c => c.OccurredOn)
            .ToHashSet();

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (!existingDates.Contains(date))
            {
                context.StreakCheckIns.Add(new StreakCheckIn { StreakId = streakId, OccurredOn = date, Kind = kind });
            }
        }

        context.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Removes every logged check-in within a date range in one go, e.g. to undo a batch of mistaken entries.
    /// </summary>
    /// <param name="streakId">The ID of the streak to remove check-ins from.</param>
    /// <param name="start">The first day to remove, inclusive.</param>
    /// <param name="end">The last day to remove, inclusive.</param>
    /// <returns>A <see cref="Result" /> indicating success, or a failure if the range is invalid.</returns>
    public Result BulkRemoveCheckIns(Guid streakId, DateOnly start, DateOnly end)
    {
        if (end < start)
        {
            return Result.Fail("The end date must be on or after the start date.");
        }

        using var context = _dbContextFactory.CreateDbContext();
        var existing = context.StreakCheckIns.Where(c => c.StreakId == streakId && c.OccurredOn >= start && c.OccurredOn <= end);
        context.StreakCheckIns.RemoveRange(existing);
        context.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Sets the kind of every already-logged check-in within a date range in one go. Unlike
    ///     <see cref="BackfillCheckIns" />, this only edits days that are already logged - it never logs a day that
    ///     isn't.
    /// </summary>
    /// <param name="streakId">The ID of the streak to edit check-ins for.</param>
    /// <param name="start">The first day to edit, inclusive.</param>
    /// <param name="end">The last day to edit, inclusive.</param>
    /// <param name="kind">The kind to set every check-in in the range to.</param>
    /// <returns>A <see cref="Result" /> indicating success, or a failure if the range is invalid.</returns>
    public Result BulkSetCheckInKind(Guid streakId, DateOnly start, DateOnly end, StreakCheckInKind kind)
    {
        if (end < start)
        {
            return Result.Fail("The end date must be on or after the start date.");
        }

        using var context = _dbContextFactory.CreateDbContext();
        var existing = context.StreakCheckIns.Where(c => c.StreakId == streakId && c.OccurredOn >= start && c.OccurredOn <= end);
        foreach (var checkIn in existing)
        {
            checkIn.Kind = kind;
        }

        context.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Logs the day a check-out streak was broken, restarting its count from that day. Logging over an
    ///     already-logged day replaces its note.
    /// </summary>
    /// <param name="streakId">The ID of the streak to log against.</param>
    /// <param name="occurredOn">The day the streak broke.</param>
    /// <param name="note">A short note about the reset, if any.</param>
    /// <returns>A <see cref="Result" /> indicating success.</returns>
    public Result LogReset(Guid streakId, DateOnly occurredOn, string? note = null)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var existing = context.StreakResets.FirstOrDefault(r => r.StreakId == streakId && r.OccurredOn == occurredOn);

        if (existing is null)
        {
            context.StreakResets.Add(new StreakReset { StreakId = streakId, OccurredOn = occurredOn, Note = note });
        }
        else
        {
            existing.Note = note;
        }

        context.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Removes a logged reset, e.g. to correct a mistaken entry.
    /// </summary>
    /// <param name="streakId">The ID of the streak to remove the reset from.</param>
    /// <param name="occurredOn">The day to un-log.</param>
    /// <returns>A <see cref="Result" /> indicating success, or a failure if no reset exists for that day.</returns>
    public Result RemoveReset(Guid streakId, DateOnly occurredOn)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var existing = context.StreakResets.FirstOrDefault(r => r.StreakId == streakId && r.OccurredOn == occurredOn);

        if (existing is null)
        {
            return Result.Fail("No reset exists for that day.");
        }

        context.StreakResets.Remove(existing);
        context.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Removes every logged reset within a date range in one go, e.g. to undo a batch of mistaken entries.
    /// </summary>
    /// <param name="streakId">The ID of the streak to remove resets from.</param>
    /// <param name="start">The first day to remove, inclusive.</param>
    /// <param name="end">The last day to remove, inclusive.</param>
    /// <returns>A <see cref="Result" /> indicating success, or a failure if the range is invalid.</returns>
    public Result BulkRemoveResets(Guid streakId, DateOnly start, DateOnly end)
    {
        if (end < start)
        {
            return Result.Fail("The end date must be on or after the start date.");
        }

        using var context = _dbContextFactory.CreateDbContext();
        var existing = context.StreakResets.Where(r => r.StreakId == streakId && r.OccurredOn >= start && r.OccurredOn <= end);
        context.StreakResets.RemoveRange(existing);
        context.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Computes a streak's current and best counts from its logged history.
    /// </summary>
    /// <param name="streak">The streak to compute stats for, with <see cref="Streak.CurrentDraft" /> loaded.</param>
    /// <returns>The streak's computed stats.</returns>
    public StreakStats GetStats(Streak streak)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (streak.Mode == StreakMode.CheckIn)
        {
            var checkIns = GetCheckIns(streak.Id);
            return ComputeCheckInStats(checkIns, streak.CadenceUnit!.Value, streak.CadenceInterval!.Value, today);
        }

        var resets = GetResets(streak.Id);
        return ComputeCheckOutStats(resets, streak.StartedOn!.Value, today);
    }

    /// <summary>
    ///     Gets the check-ins making up a check-in streak's current run, oldest first - the same trailing run
    ///     <see cref="GetStats" /> counts from, but as the actual logged days rather than just their count, so a
    ///     caller can tell which of them were frozen rather than completed.
    /// </summary>
    /// <param name="streak">The streak, with <see cref="Streak.CurrentDraft" /> loaded.</param>
    /// <returns>The current run's check-ins, oldest first, or an empty list if the streak is currently broken.</returns>
    public IReadOnlyList<StreakCheckIn> GetCurrentRunCheckIns(Streak streak)
    {
        var checkIns = GetCheckIns(streak.Id);
        if (checkIns.Count == 0)
        {
            return [];
        }

        var unit = streak.CadenceUnit!.Value;
        var interval = streak.CadenceInterval!.Value;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (today > NextDue(checkIns[^1].OccurredOn, unit, interval))
        {
            return [];
        }

        var run = new List<StreakCheckIn> { checkIns[^1] };
        for (var i = checkIns.Count - 2; i >= 0; i--)
        {
            if (checkIns[i + 1].OccurredOn > NextDue(checkIns[i].OccurredOn, unit, interval))
            {
                break;
            }

            run.Add(checkIns[i]);
        }

        run.Reverse();
        return run;
    }

    /// <summary>
    ///     Computes the date by which the next check-in of a check-in streak is due.
    /// </summary>
    /// <param name="from">The date to count forward from.</param>
    /// <param name="unit">The cadence unit.</param>
    /// <param name="interval">The number of <paramref name="unit" />s the next check-in is due within.</param>
    /// <returns>The due date. On or before this date, the streak is still alive.</returns>
    private static DateOnly NextDue(DateOnly from, StreakCadenceUnit unit, int interval)
    {
        return unit switch
        {
            StreakCadenceUnit.Day => from.AddDays(interval),
            StreakCadenceUnit.Week => from.AddDays(interval * 7),
            StreakCadenceUnit.Month => from.AddMonths(interval),
            StreakCadenceUnit.Year => from.AddYears(interval),
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "Unknown cadence unit")
        };
    }

    /// <summary>
    ///     Computes a check-in streak's current and best counts. A gap larger than its cadence breaks the run it
    ///     falls in; a frozen day bridges a gap without adding to the count.
    /// </summary>
    private static StreakStats ComputeCheckInStats(IReadOnlyList<StreakCheckIn> checkIns, StreakCadenceUnit unit, int interval,
        DateOnly today)
    {
        if (checkIns.Count == 0)
        {
            return new StreakStats(0, 0);
        }

        // checkIns is already sorted oldest-first by GetCheckIns
        var best = 0;
        var runCompletedCount = 0;

        for (var i = 0; i < checkIns.Count; i++)
        {
            if (i > 0 && checkIns[i].OccurredOn > NextDue(checkIns[i - 1].OccurredOn, unit, interval))
            {
                best = Math.Max(best, runCompletedCount);
                runCompletedCount = 0;
            }

            if (checkIns[i].Kind == StreakCheckInKind.Completed)
            {
                runCompletedCount++;
            }
        }

        best = Math.Max(best, runCompletedCount);

        var lastLoggedDay = checkIns[^1].OccurredOn;
        var current = today <= NextDue(lastLoggedDay, unit, interval) ? runCompletedCount : 0;

        return new StreakStats(current, best);
    }

    /// <summary>
    ///     Computes a check-out streak's current and best counts, in days, from its logged resets.
    /// </summary>
    private static StreakStats ComputeCheckOutStats(IReadOnlyList<StreakReset> resets, DateOnly startedOn, DateOnly today)
    {
        // resets is already sorted oldest-first by GetResets
        var best = 0;
        var boundary = startedOn;

        foreach (var reset in resets)
        {
            best = Math.Max(best, reset.OccurredOn.DayNumber - boundary.DayNumber);
            boundary = reset.OccurredOn;
        }

        var current = today.DayNumber - boundary.DayNumber;
        best = Math.Max(best, current);

        return new StreakStats(current, best);
    }

    /// <summary>
    ///     Builds a new, unsaved draft snapshot for the specified streak.
    /// </summary>
    private static StreakDraft NewDraft(Guid streakId, StreakDraftContent content)
    {
        return new StreakDraft
        {
            StreakId = streakId,
            Title = content.Title,
            Body = content.Body,
            Visibility = content.Visibility,
            Color = content.Color,
            DotColor = content.DotColor,
            CadenceUnit = content.CadenceUnit,
            CadenceInterval = content.CadenceInterval
        };
    }

    /// <summary>
    ///     Returns a value indicating whether the given slug is already in use by another streak.
    /// </summary>
    private static bool SlugInUse(AppDbContext context, string slug, Guid? excludingId)
    {
        return context.Streaks.Any(s => s.Slug == slug && s.Id != excludingId);
    }
}
