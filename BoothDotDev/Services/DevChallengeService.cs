using System.Diagnostics.CodeAnalysis;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using DEDrake;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service which fetches and manages dev challenges.
/// </summary>
public sealed class DevChallengeService
{
    private const string Area = "challenge";

    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly CdnMediaService _cdnMediaService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DevChallengeService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The factory for creating the web database context.</param>
    /// <param name="cdnMediaService">The <see cref="CdnMediaService" />.</param>
    public DevChallengeService(IDbContextFactory<AppDbContext> dbContextFactory, CdnMediaService cdnMediaService)
    {
        _dbContextFactory = dbContextFactory;
        _cdnMediaService = cdnMediaService;
    }

    /// <summary>
    ///     Creates a new challenge, along with its first draft, which immediately becomes the challenge's current
    ///     draft.
    /// </summary>
    /// <param name="request">The challenge's parent-level fields and the content of its first draft.</param>
    /// <returns>A <see cref="Result{T}" /> containing the newly-created challenge.</returns>
    public Result<DevChallenge> CreateChallenge(DevChallengeSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var challenge = new DevChallenge
        {
            PublishedAt = request.PublishedAt.ToUniversalTime()
        };

        // two SaveChanges calls, not one: DevChallenge -> DevChallengeDraft (via DevChallengeId) and
        // DevChallengeDraft -> DevChallenge (via CurrentDraftId) form a cycle between two rows that are
        // both new, which EF can't resolve in a single call even though CurrentDraftId is nullable.
        context.DevChallenges.Add(challenge);
        context.SaveChanges();

        var draft = NewDraft(challenge.Id, request.Content);
        context.DevChallengeDrafts.Add(draft);
        challenge.CurrentDraftId = draft.Id;
        context.SaveChanges();

        return challenge;
    }

    /// <summary>
    ///     Saves a new draft of an existing challenge, without publishing it.
    /// </summary>
    /// <param name="id">The ID of the challenge to save a draft for.</param>
    /// <param name="request">The challenge's parent-level fields and the content of the new draft.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the challenge the draft was saved for, or an error if no challenge
    ///     with the specified <paramref name="id" /> exists.
    /// </returns>
    public Result<DevChallenge> SaveDraft(ShortGuid id, DevChallengeSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var challenge = context.DevChallenges.Find(id);

        if (challenge is null)
        {
            return Result.Fail($"Challenge with ID '{id}' not found.");
        }

        var draft = NewDraft(challenge.Id, request.Content);
        context.DevChallengeDrafts.Add(draft);

        challenge.PublishedAt = request.PublishedAt.ToUniversalTime();

        context.SaveChanges();
        return challenge;
    }

    /// <summary>
    ///     Saves a new draft of an existing challenge and publishes it, making it the challenge's current draft.
    /// </summary>
    /// <param name="id">The ID of the challenge to publish.</param>
    /// <param name="request">The challenge's parent-level fields and the content of the new draft.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the updated challenge, or an error if no challenge with the
    ///     specified <paramref name="id" /> exists.
    /// </returns>
    public Result<DevChallenge> PublishChallenge(ShortGuid id, DevChallengeSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var challenge = context.DevChallenges.Find(id);

        if (challenge is null)
        {
            return Result.Fail($"Challenge with ID '{id}' not found.");
        }

        var draft = NewDraft(challenge.Id, request.Content);
        context.DevChallengeDrafts.Add(draft);

        challenge.PublishedAt = request.PublishedAt.ToUniversalTime();
        challenge.CurrentDraftId = draft.Id;
        challenge.UpdatedAt = DateTimeOffset.UtcNow;

        context.SaveChanges();
        return challenge;
    }

    /// <summary>
    ///     Moves a challenge to the trash. It's excluded from every listing and 404s on its public URL, but nothing
    ///     about it is otherwise touched, and it can be restored with <see cref="RestoreChallenge" />.
    /// </summary>
    /// <param name="id">The ID of the challenge to trash.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the trashed challenge, or an error if no challenge with the
    ///     specified <paramref name="id" /> exists.
    /// </returns>
    public Result<DevChallenge> TrashChallenge(ShortGuid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var challenge = context.DevChallenges.Find(id);

        if (challenge is null)
        {
            return Result.Fail($"The challenge with ID {id} was not found");
        }

        challenge.TrashedAt = DateTimeOffset.UtcNow;
        context.SaveChanges();
        return challenge;
    }

    /// <summary>
    ///     Restores a trashed challenge, making it visible in listings and on its public URL again.
    /// </summary>
    /// <param name="id">The ID of the challenge to restore.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the restored challenge, or an error if no challenge with the
    ///     specified <paramref name="id" /> exists.
    /// </returns>
    public Result<DevChallenge> RestoreChallenge(ShortGuid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var challenge = context.DevChallenges.Find(id);

        if (challenge is null)
        {
            return Result.Fail($"The challenge with ID {id} was not found");
        }

        challenge.TrashedAt = null;
        context.SaveChanges();
        return challenge;
    }

    /// <summary>
    ///     Permanently deletes a trashed challenge - the challenge row, every draft in its revision history (cascade), and every
    ///     file it had uploaded to the CDN. This cannot be undone.
    /// </summary>
    /// <param name="id">The ID of the challenge to permanently delete.</param>
    /// <returns>
    ///     A <see cref="Result" /> indicating success, or a failure if no challenge with the specified <paramref name="id" />
    ///     exists or it isn't currently trashed.
    /// </returns>
    public Result PermanentlyDeleteChallenge(ShortGuid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var challenge = context.DevChallenges.Find(id);

        if (challenge is null)
        {
            return Result.Fail($"The challenge with ID {id} was not found");
        }

        if (challenge.TrashedAt is null)
        {
            return Result.Fail("Only trashed challenges can be permanently deleted.");
        }

        _cdnMediaService.DeleteAllMedia(id, challenge.PublishedAt, Area);

        context.DevChallenges.Remove(challenge);
        context.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Gets a read-only collection of dev challenges, in challenge order (oldest first).
    /// </summary>
    /// <param name="visibility">The visibility of the dev challenges.</param>
    /// <returns>A read-only collection of dev challenges, excluding trashed ones.</returns>
    public IReadOnlyList<DevChallenge> GetDevChallenges(Visibility visibility)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        IQueryable<DevChallenge> challenges = context.DevChallenges.Include(c => c.CurrentDraft)
            .Where(c => c.TrashedAt == null)
            .OrderBy(c => c.PublishedAt);

        if (visibility != Visibility.None)
        {
            challenges = challenges.Where(c => c.CurrentDraft!.Visibility == visibility);
        }

        return [.. challenges];
    }

    /// <summary>
    ///     Gets all challenges, newest first.
    /// </summary>
    /// <param name="visibility">The visibility of the challenges to retrieve.</param>
    /// <returns>A read-only view of all challenges, excluding trashed ones.</returns>
    public IReadOnlyList<DevChallenge> GetAllChallenges(Visibility visibility = Visibility.Published)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var challenges = context.DevChallenges.Include(c => c.CurrentDraft).Where(c => c.TrashedAt == null);
        if (visibility != Visibility.None)
        {
            challenges = challenges.Where(c => c.CurrentDraft!.Visibility == visibility);
        }

        return [.. challenges.OrderByDescending(c => c.PublishedAt)];
    }

    /// <summary>
    ///     Gets a challenge by its ID.
    /// </summary>
    /// <param name="id">The ID of the challenge.</param>
    /// <param name="includeTrashed">
    ///     Whether to include the challenge if it's trashed. Only the admin editor should pass <see langword="true" />
    ///     - every public-facing caller should get the trash exclusion for free.
    /// </param>
    /// <returns>A <see cref="Result{T}" /> containing the challenge if found; otherwise, an error result.</returns>
    public Result<DevChallenge> GetChallengeById(ShortGuid id, bool includeTrashed = false)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var challenge = context.DevChallenges.Include(c => c.CurrentDraft).FirstOrDefault(c => c.Id == id);

        if (challenge is null || (challenge.TrashedAt is not null && !includeTrashed))
        {
            return Result.Fail($"The challenge with ID {id} was not found");
        }

        return challenge;
    }

    /// <summary>
    ///     Returns the most recent dev challenges, limited to the specified count.
    /// </summary>
    /// <param name="searchOptions">The options for searching and retrieving dev challenges.</param>
    /// <returns>A read-only list of the most recent dev challenges, excluding trashed ones.</returns>
    public IReadOnlyList<DevChallenge> GetRecentChallenges(ActivitySearchOptions searchOptions)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var challenges = context.DevChallenges.Include(c => c.CurrentDraft).Where(c => c.TrashedAt == null);

        if (searchOptions.Visibility != Visibility.None)
        {
            challenges = challenges.Where(c => c.CurrentDraft!.Visibility == searchOptions.Visibility);
        }

        var ordered = searchOptions.SortStrategy switch
        {
            ActivitySortStrategy.Published => challenges.OrderByDescending(c => c.PublishedAt),
            ActivitySortStrategy.Updated => challenges.OrderByDescending(c => c.UpdatedAt ?? c.PublishedAt),
            _ => throw new ArgumentOutOfRangeException(nameof(searchOptions), searchOptions.SortStrategy, "Unknown sort strategy")
        };

        return [.. ordered.Take(searchOptions.Count)];
    }

    /// <summary>
    ///     Gets all trashed challenges, newest-trashed first.
    /// </summary>
    /// <returns>A read-only view of all trashed challenges.</returns>
    public IReadOnlyList<DevChallenge> GetTrashedChallenges()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return
        [
            .. context.DevChallenges.Include(c => c.CurrentDraft).Where(c => c.TrashedAt != null)
                .OrderByDescending(c => c.TrashedAt)
        ];
    }

    /// <summary>
    ///     Tries to get a dev challenge by its ID.
    /// </summary>
    /// <param name="id">The ID of the dev challenge.</param>
    /// <param name="devChallenge">
    ///     When this method returns, contains the dev challenge associated with the specified id, if the id is found;
    ///     otherwise, the default value for the type will be returned. This parameter is passed uninitialized.
    /// </param>
    /// <param name="shouldRedirect">
    ///     When this method returns, contains a value indicating whether the user should be redirected to the new URL.
    ///     This parameter is passed uninitialized.
    /// </param>
    /// <returns><see langword="true" /> if the dev challenge is found; otherwise, <see langword="false" />.</returns>
    public bool TryGetDevChallenge(string id,
        [NotNullWhen(true)] out DevChallenge? devChallenge,
        out bool shouldRedirect)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            devChallenge = null;
            shouldRedirect = false;
            return false;
        }

        using var context = _dbContextFactory.CreateDbContext();
        if (int.TryParse(id, out var oldId))
        {
            devChallenge = context.DevChallenges.Include(c => c.CurrentDraft)
                .FirstOrDefault(c => c.OldId == oldId && c.TrashedAt == null);
            shouldRedirect = devChallenge is not null;
            return shouldRedirect;
        }

        ShortGuid guid;

        try
        {
            guid = ShortGuid.Parse(id);
        }
        catch (FormatException)
        {
            devChallenge = null;
            shouldRedirect = false;
            return false;
        }

        devChallenge = context.DevChallenges.Include(c => c.CurrentDraft)
            .FirstOrDefault(c => c.Id == guid && c.TrashedAt == null);
        shouldRedirect = false;
        return devChallenge is not null;
    }

    /// <summary>
    ///     Returns a challenge's full draft history, newest first.
    /// </summary>
    /// <param name="id">The ID of the challenge whose draft history to return.</param>
    /// <returns>The challenge's drafts, newest first.</returns>
    public IReadOnlyList<DevChallengeDraft> GetDraftHistory(ShortGuid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return [.. context.DevChallengeDrafts.Where(d => d.DevChallengeId == id).OrderByDescending(d => d.CreatedAt)];
    }

    /// <summary>
    ///     Returns a specific draft of the specified challenge, for viewing without publishing it.
    /// </summary>
    /// <param name="id">The ID of the challenge the draft belongs to.</param>
    /// <param name="draftId">The ID of the draft to return.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the requested draft, or an error if it doesn't exist or doesn't
    ///     belong to the specified challenge.
    /// </returns>
    public Result<DevChallengeDraft> GetDraft(ShortGuid id, Guid draftId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var draft = context.DevChallengeDrafts.Find(draftId);

        if (draft is null || draft.DevChallengeId != id)
        {
            return Result.Fail($"Draft '{draftId}' not found for challenge '{id}'.");
        }

        return draft;
    }

    /// <summary>
    ///     Returns the newest draft of the specified challenge, which may or may not be the challenge's current
    ///     (published) draft.
    /// </summary>
    /// <param name="id">The ID of the challenge whose newest draft to return.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the challenge's newest draft, or an error if the challenge has no
    ///     drafts.
    /// </returns>
    public Result<DevChallengeDraft> GetNewestDraft(ShortGuid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var draft = context.DevChallengeDrafts.Where(d => d.DevChallengeId == id)
            .OrderByDescending(d => d.CreatedAt)
            .FirstOrDefault();

        if (draft is null)
        {
            return Result.Fail($"Challenge '{id}' has no drafts.");
        }

        return draft;
    }

    /// <summary>
    ///     Builds a new, unsaved draft snapshot for the specified challenge.
    /// </summary>
    private static DevChallengeDraft NewDraft(ShortGuid devChallengeId, DevChallengeDraftContent content)
    {
        return new DevChallengeDraft
        {
            DevChallengeId = devChallengeId,
            Title = content.Title,
            Description = content.Description,
            Excerpt = content.Excerpt,
            Solution = content.Solution,
            ShowSolution = content.ShowSolution,
            Visibility = content.Visibility
        };
    }
}
