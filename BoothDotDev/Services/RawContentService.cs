using BoothDotDev.Data;
using Cysharp.Text;
using DEDrake;
using FluentResults;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for building the raw Markdown source of a piece of content, as served from its
///     <c>.md</c> URL.
/// </summary>
public sealed class RawContentService
{
    private readonly BlogPostService _blogPostService;
    private readonly DevChallengeService _devChallengeService;
    private readonly NoteService _noteService;
    private readonly ProjectService _projectService;
    private readonly TutorialService _tutorialService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RawContentService" /> class.
    /// </summary>
    /// <param name="blogPostService">The <see cref="BlogPostService" />.</param>
    /// <param name="noteService">The <see cref="NoteService" />.</param>
    /// <param name="tutorialService">The <see cref="TutorialService" />.</param>
    /// <param name="projectService">The <see cref="ProjectService" />.</param>
    /// <param name="devChallengeService">The <see cref="DevChallengeService" />.</param>
    public RawContentService(
        BlogPostService blogPostService,
        NoteService noteService,
        TutorialService tutorialService,
        ProjectService projectService,
        DevChallengeService devChallengeService)
    {
        _blogPostService = blogPostService;
        _noteService = noteService;
        _tutorialService = tutorialService;
        _projectService = projectService;
        _devChallengeService = devChallengeService;
    }

    /// <summary>
    ///     Builds the raw Markdown source of a blog post.
    /// </summary>
    /// <param name="slug">The slug of the blog post.</param>
    /// <param name="isAuthenticated">Whether the requesting user is signed in, granting access to private posts.</param>
    /// <returns>A <see cref="Result{T}" /> containing the raw Markdown, or an error if the post can't be shown.</returns>
    public Result<string> BuildBlogPostRaw(string slug, bool isAuthenticated)
    {
        var result = _blogPostService.GetPost(slug);
        if (result.IsFailed)
        {
            return Result.Fail($"Blog post '{slug}' not found.");
        }

        var post = result.Value;
        if (post.Visibility == Visibility.Private && !isAuthenticated)
        {
            return Result.Fail($"Blog post '{slug}' not found.");
        }

        return BuildRaw(post.Title, post.Body, post.PublishedAt, post.UpdatedAt, post.Author.DisplayName);
    }

    /// <summary>
    ///     Builds the raw Markdown source of a note.
    /// </summary>
    /// <param name="id">The ID of the note.</param>
    /// <param name="isAuthenticated">Whether the requesting user is signed in, granting access to private notes.</param>
    /// <returns>A <see cref="Result{T}" /> containing the raw Markdown, or an error if the note can't be shown.</returns>
    public Result<string> BuildNoteRaw(string id, bool isAuthenticated)
    {
        if (!TryParseShortGuid(id, out var guid))
        {
            return Result.Fail($"Note '{id}' not found.");
        }

        var result = _noteService.GetNoteById(guid);
        if (result.IsFailed)
        {
            return Result.Fail($"Note '{id}' not found.");
        }

        var note = result.Value;
        if (note.Visibility == Visibility.Private && !isAuthenticated)
        {
            return Result.Fail($"Note '{id}' not found.");
        }

        return BuildRaw(note.Title, note.Content, note.PublishedAt, note.UpdatedAt);
    }

    /// <summary>
    ///     Builds the raw Markdown source of a tutorial article.
    /// </summary>
    /// <param name="slug">The full slug of the article, including its folder path.</param>
    /// <param name="isAuthenticated">Whether the requesting user is signed in, granting access to private articles.</param>
    /// <returns>A <see cref="Result{T}" /> containing the raw Markdown, or an error if the article can't be shown.</returns>
    public Result<string> BuildTutorialRaw(string slug, bool isAuthenticated)
    {
        var result = _tutorialService.GetArticle(slug);
        if (result.IsFailed)
        {
            return Result.Fail($"Tutorial article '{slug}' not found.");
        }

        var article = result.Value;
        if (article.Visibility == Visibility.Private && !isAuthenticated)
        {
            return Result.Fail($"Tutorial article '{slug}' not found.");
        }

        return BuildRaw(article.Title, article.Body, article.PublishedAt, article.UpdatedAt);
    }

    /// <summary>
    ///     Builds the raw Markdown source of a project devlog entry.
    /// </summary>
    /// <param name="projectSlug">The slug of the devlog's parent project.</param>
    /// <param name="devlogSlug">The slug of the devlog entry.</param>
    /// <param name="isAuthenticated">Whether the requesting user is signed in, granting access to private devlogs.</param>
    /// <returns>A <see cref="Result{T}" /> containing the raw Markdown, or an error if the devlog can't be shown.</returns>
    public Result<string> BuildDevlogRaw(string projectSlug, string devlogSlug, bool isAuthenticated)
    {
        if (!_projectService.TryGetProject(projectSlug, out var project))
        {
            return Result.Fail($"Devlog '{projectSlug}/{devlogSlug}' not found.");
        }

        if (!_projectService.TryGetDevlog(project, devlogSlug, out var devlog))
        {
            return Result.Fail($"Devlog '{projectSlug}/{devlogSlug}' not found.");
        }

        if (devlog.Visibility == Visibility.Private && !isAuthenticated)
        {
            return Result.Fail($"Devlog '{projectSlug}/{devlogSlug}' not found.");
        }

        return BuildRaw(devlog.Title, devlog.Body, devlog.PublishedAt, devlog.UpdatedAt);
    }

    /// <summary>
    ///     Builds the raw Markdown source of a dev challenge.
    /// </summary>
    /// <param name="id">The ID of the challenge.</param>
    /// <param name="isAuthenticated">Whether the requesting user is signed in, granting access to private challenges.</param>
    /// <returns>A <see cref="Result{T}" /> containing the raw Markdown, or an error if the challenge can't be shown.</returns>
    public Result<string> BuildChallengeRaw(string id, bool isAuthenticated)
    {
        if (!TryParseShortGuid(id, out var guid))
        {
            return Result.Fail($"Challenge '{id}' not found.");
        }

        var result = _devChallengeService.GetChallengeById(guid);
        if (result.IsFailed)
        {
            return Result.Fail($"Challenge '{id}' not found.");
        }

        var challenge = result.Value;
        if (challenge.Visibility == Visibility.Private && !isAuthenticated)
        {
            return Result.Fail($"Challenge '{id}' not found.");
        }

        var raw = BuildRaw(challenge.Title, challenge.Description, challenge.PublishedAt, challenge.UpdatedAt);
        if (!challenge.ShowSolution || string.IsNullOrWhiteSpace(challenge.Solution))
        {
            return raw;
        }

        using var builder = ZString.CreateUtf8StringBuilder();
        builder.Append(raw);
        builder.AppendLine();
        builder.AppendLine("---");
        builder.AppendLine();
        builder.AppendLine("## Solution");
        builder.AppendLine();
        builder.AppendLine(challenge.Solution);
        return builder.ToString();
    }

    private static bool TryParseShortGuid(string id, out Guid guid)
    {
        try
        {
            guid = ShortGuid.Parse(id);
            return true;
        }
        catch (FormatException)
        {
            guid = default;
            return false;
        }
    }

    private static string BuildRaw(string title, string body, DateTimeOffset publishedAt, DateTimeOffset? updatedAt,
        string? author = null)
    {
        using var builder = ZString.CreateUtf8StringBuilder();
        builder.AppendLine("# " + title);

        if (author is not null)
        {
            builder.AppendLine($"Author: {author}");
        }

        builder.AppendLine($"Published: {publishedAt:R}");
        if (updatedAt.HasValue)
        {
            builder.AppendLine($"Updated: {updatedAt:R}");
        }

        builder.AppendLine();
        builder.AppendLine(body);
        return builder.ToString();
    }
}
