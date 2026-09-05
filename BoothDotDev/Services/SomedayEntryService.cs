using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for managing the entries on the "someday" page.
/// </summary>
public sealed class SomedayEntryService
{
    private const string Area = "someday";
    private readonly CdnMediaService _cdnMediaService;

    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SomedayEntryService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="IDbContextFactory{TContext}" />.</param>
    /// <param name="cdnMediaService">The <see cref="CdnMediaService" />.</param>
    public SomedayEntryService(IDbContextFactory<AppDbContext> dbContextFactory, CdnMediaService cdnMediaService)
    {
        _dbContextFactory = dbContextFactory;
        _cdnMediaService = cdnMediaService;
    }

    /// <summary>
    ///     Creates a new someday entry, along with its first draft, which immediately becomes the entry's current
    ///     draft. The entry is appended to the end of the current sort order.
    /// </summary>
    /// <param name="request">The entry's parent-level fields and the content of its first draft.</param>
    /// <returns>A <see cref="Result{T}" /> containing the newly-created entry.</returns>
    public Result<SomedayEntry> CreateEntry(SomedayEntrySaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();

        if (SlugInUse(context, request.Slug, null))
        {
            return Result.Fail($"Slug '{request.Slug}' is already in use.");
        }

        var entry = new SomedayEntry { Slug = request.Slug, SortOrder = request.SortOrder };

        // two SaveChanges calls, not one: SomedayEntry -> SomedayEntryDraft (via SomedayEntryId) and
        // SomedayEntryDraft -> SomedayEntry (via CurrentDraftId) form a cycle between two rows that are both
        // new, which EF can't resolve in a single call even though CurrentDraftId is nullable.
        context.SomedayEntries.Add(entry);
        context.SaveChanges();

        var draft = NewDraft(entry.Id, request.Content);
        context.SomedayEntryDrafts.Add(draft);
        entry.CurrentDraftId = draft.Id;
        context.SaveChanges();

        return entry;
    }

    /// <summary>
    ///     Saves a new draft of an existing someday entry, without publishing it. The entry's currently-live
    ///     draft, if any, is left unchanged, so the public page is unaffected.
    /// </summary>
    /// <param name="id">The ID of the entry to save a draft for.</param>
    /// <param name="request">The entry's parent-level fields and the content of the new draft.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the entry the draft was saved for, or an error if no entry with
    ///     the specified <paramref name="id" /> exists.
    /// </returns>
    public Result<SomedayEntry> SaveDraft(Guid id, SomedayEntrySaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var entry = context.SomedayEntries.Find(id);

        if (entry is null)
        {
            return Result.Fail($"Someday entry with ID '{id}' not found.");
        }

        if (SlugInUse(context, request.Slug, id))
        {
            return Result.Fail($"Slug '{request.Slug}' is already in use.");
        }

        var draft = NewDraft(entry.Id, request.Content);
        context.SomedayEntryDrafts.Add(draft);

        entry.Slug = request.Slug;
        entry.SortOrder = request.SortOrder;

        context.SaveChanges();
        return entry;
    }

    /// <summary>
    ///     Saves a new draft of an existing someday entry and publishes it, making it the entry's current draft.
    /// </summary>
    /// <param name="id">The ID of the entry to publish.</param>
    /// <param name="request">The entry's parent-level fields and the content of the new draft.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the updated entry, or an error if no entry with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<SomedayEntry> PublishEntry(Guid id, SomedayEntrySaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var entry = context.SomedayEntries.Find(id);

        if (entry is null)
        {
            return Result.Fail($"Someday entry with ID '{id}' not found.");
        }

        if (SlugInUse(context, request.Slug, id))
        {
            return Result.Fail($"Slug '{request.Slug}' is already in use.");
        }

        var draft = NewDraft(entry.Id, request.Content);
        context.SomedayEntryDrafts.Add(draft);

        entry.Slug = request.Slug;
        entry.SortOrder = request.SortOrder;
        entry.CurrentDraftId = draft.Id;
        entry.UpdatedAt = DateTimeOffset.UtcNow;

        context.SaveChanges();
        return entry;
    }

    /// <summary>
    ///     Moves a someday entry to the trash. It's excluded from the public page, but nothing about it is
    ///     otherwise touched, and it can be restored with <see cref="RestoreEntry" />.
    /// </summary>
    /// <param name="id">The ID of the entry to trash.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the trashed entry, or an error if no entry with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<SomedayEntry> TrashEntry(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var entry = context.SomedayEntries.Find(id);

        if (entry is null)
        {
            return Result.Fail($"The someday entry with ID {id} was not found");
        }

        entry.TrashedAt = DateTimeOffset.UtcNow;
        context.SaveChanges();
        return entry;
    }

    /// <summary>
    ///     Restores a trashed someday entry, making it visible on the public page again.
    /// </summary>
    /// <param name="id">The ID of the entry to restore.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the restored entry, or an error if no entry with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<SomedayEntry> RestoreEntry(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var entry = context.SomedayEntries.Find(id);

        if (entry is null)
        {
            return Result.Fail($"The someday entry with ID {id} was not found");
        }

        entry.TrashedAt = null;
        context.SaveChanges();
        return entry;
    }

    /// <summary>
    ///     Permanently deletes a trashed someday entry - the entry row, every draft in its revision history
    ///     (cascade), and every file it had uploaded to the CDN. This cannot be undone.
    /// </summary>
    /// <param name="id">The ID of the entry to permanently delete.</param>
    /// <returns>
    ///     A <see cref="Result" /> indicating success, or a failure if no entry with the specified
    ///     <paramref name="id" /> exists or it isn't currently trashed.
    /// </returns>
    public Result PermanentlyDeleteEntry(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var entry = context.SomedayEntries.Find(id);

        if (entry is null)
        {
            return Result.Fail($"The someday entry with ID {id} was not found");
        }

        if (entry.TrashedAt is null)
        {
            return Result.Fail("Only trashed entries can be permanently deleted.");
        }

        _cdnMediaService.DeleteAllMedia(id, entry.PublishedAt, Area);

        context.SomedayEntries.Remove(entry);
        context.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Gets every someday entry, in curated display order, for the admin listing.
    /// </summary>
    /// <returns>A read-only view of every entry in <see cref="SomedayEntry.SortOrder" />, excluding trashed ones.</returns>
    public IReadOnlyList<SomedayEntry> GetAllEntries()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return
        [
            .. context.SomedayEntries.Include(e => e.CurrentDraft)
                .Where(e => e.TrashedAt == null)
                .OrderBy(e => e.SortOrder)
        ];
    }

    /// <summary>
    ///     Gets every published, non-trashed someday entry, in curated display order, for the public page.
    /// </summary>
    /// <returns>A read-only view of every published entry, in <see cref="SomedayEntry.SortOrder" />.</returns>
    public IReadOnlyList<SomedayEntry> GetPublishedEntries()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return
        [
            .. context.SomedayEntries.Include(e => e.CurrentDraft)
                .Where(e => e.TrashedAt == null && e.CurrentDraft!.Visibility == Visibility.Published)
                .OrderBy(e => e.SortOrder)
        ];
    }

    /// <summary>
    ///     Gets a someday entry by its ID.
    /// </summary>
    /// <param name="id">The ID of the entry.</param>
    /// <param name="includeTrashed">
    ///     Whether to include the entry if it's trashed. Only the admin editor should pass <see langword="true" /> -
    ///     every public-facing caller should get the trash exclusion for free.
    /// </param>
    /// <returns>A <see cref="Result{T}" /> containing the entry if found; otherwise, an error result.</returns>
    public Result<SomedayEntry> GetEntryById(Guid id, bool includeTrashed = false)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var entry = context.SomedayEntries.Include(e => e.CurrentDraft).FirstOrDefault(e => e.Id == id);

        if (entry is null || (entry.TrashedAt is not null && !includeTrashed))
        {
            return Result.Fail($"The someday entry with ID {id} was not found");
        }

        return entry;
    }

    /// <summary>
    ///     Gets all trashed someday entries, newest-trashed first.
    /// </summary>
    /// <returns>A read-only view of all trashed entries.</returns>
    public IReadOnlyList<SomedayEntry> GetTrashedEntries()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return
        [
            .. context.SomedayEntries.Include(e => e.CurrentDraft)
                .Where(e => e.TrashedAt != null)
                .OrderByDescending(e => e.TrashedAt)
        ];
    }

    /// <summary>
    ///     Returns a someday entry's full draft history, newest first.
    /// </summary>
    /// <param name="id">The ID of the entry whose draft history to return.</param>
    /// <returns>The entry's drafts, newest first.</returns>
    public IReadOnlyList<SomedayEntryDraft> GetDraftHistory(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return [.. context.SomedayEntryDrafts.Where(d => d.SomedayEntryId == id).OrderByDescending(d => d.CreatedAt)];
    }

    /// <summary>
    ///     Returns a specific draft of the specified someday entry, for viewing without publishing it.
    /// </summary>
    /// <param name="id">The ID of the entry the draft belongs to.</param>
    /// <param name="draftId">The ID of the draft to return.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the requested draft, or an error if it doesn't exist or doesn't
    ///     belong to the specified entry.
    /// </returns>
    public Result<SomedayEntryDraft> GetDraft(Guid id, Guid draftId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var draft = context.SomedayEntryDrafts.Find(draftId);

        if (draft is null || draft.SomedayEntryId != id)
        {
            return Result.Fail($"Draft '{draftId}' not found for someday entry '{id}'.");
        }

        return draft;
    }

    /// <summary>
    ///     Returns the newest draft of the specified someday entry, which may or may not be the entry's current
    ///     (published) draft.
    /// </summary>
    /// <param name="id">The ID of the entry whose newest draft to return.</param>
    /// <returns>A <see cref="Result{T}" /> containing the entry's newest draft, or an error if it has no drafts.</returns>
    public Result<SomedayEntryDraft> GetNewestDraft(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var draft = context.SomedayEntryDrafts.Where(d => d.SomedayEntryId == id).OrderByDescending(d => d.CreatedAt)
            .FirstOrDefault();

        if (draft is null)
        {
            return Result.Fail($"Someday entry '{id}' has no drafts.");
        }

        return draft;
    }

    /// <summary>
    ///     Reassigns every entry's <see cref="SomedayEntry.SortOrder" /> to match its position in
    ///     <paramref name="orderedIds" />, for the admin drag-to-reorder list.
    /// </summary>
    /// <param name="orderedIds">Every non-trashed entry's ID, in its new display order.</param>
    /// <returns>A <see cref="Result" /> indicating success.</returns>
    public Result Reorder(IReadOnlyList<Guid> orderedIds)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var entries = context.SomedayEntries.Where(e => orderedIds.Contains(e.Id)).ToDictionary(e => e.Id);

        for (var i = 0; i < orderedIds.Count; i++)
        {
            if (entries.TryGetValue(orderedIds[i], out var entry))
            {
                entry.SortOrder = i;
            }
        }

        context.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Builds a new, unsaved draft snapshot for the specified someday entry.
    /// </summary>
    private static SomedayEntryDraft NewDraft(Guid entryId, SomedayEntryDraftContent content)
    {
        return new SomedayEntryDraft
        {
            SomedayEntryId = entryId, Title = content.Title, Body = content.Body, Visibility = content.Visibility
        };
    }

    /// <summary>
    ///     Returns a value indicating whether the given slug is already in use by another entry.
    /// </summary>
    private static bool SlugInUse(AppDbContext context, string slug, Guid? excludingId)
    {
        return context.SomedayEntries.Any(e => e.Slug == slug && e.Id != excludingId);
    }
}
