using BoothDotDev.Extensions;
using BoothDotDev.Services;
using DEDrake;
using Optional;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Provides factory methods for creating <see cref="ActivityEntry" /> instances from various data models.
/// </summary>
public static class ActivityEntryFactory
{
    /// <summary>
    ///     Creates an <see cref="ActivityEntry" /> from a <see cref="BlogPost" />.
    /// </summary>
    /// <param name="post">The <see cref="BlogPost" />.</param>
    /// <returns>An <see cref="ActivityEntry" /> representing the <paramref name="post" />.</returns>
    public static ActivityEntry From(BlogPost post)
    {
        return new ActivityEntry
        {
            CreatedAt = post.Published,
            Title = post.Title,
            CommitSha = post.Id.ToCommitSha(),
            PagePath = "/Blog/Article",
            Category = "blog",
            RouteValues = new Dictionary<string, string> { ["slug"] = post.Slug },
            ReadingMinutes = Option.Some(post.GetEstimatedReadingTime()),
            Visibility = post.Visibility
        };
    }

    /// <summary>
    ///     Creates an <see cref="ActivityEntry" /> from a <see cref="TutorialArticle" />.
    /// </summary>
    /// <param name="article">The <see cref="TutorialArticle" />.</param>
    /// <param name="tutorialService">The <see cref="TutorialService" />.</param>
    /// <returns>An <see cref="ActivityEntry" /> representing the <paramref name="article" />.</returns>
    public static ActivityEntry From(TutorialArticle article, TutorialService tutorialService)
    {
        return new ActivityEntry
        {
            CreatedAt = article.Published,
            Title = article.Title,
            CommitSha = article.Id.ToCommitSha(),
            PagePath = "/Learn/Tutorials/Index",
            Category = "tutorial",
            RouteValues = new Dictionary<string, string> { ["slug"] = tutorialService.GetFullSlug(article) },
            ReadingMinutes = Option.Some(article.GetEstimatedReadingTime()),
            Visibility = article.Visibility
        };
    }

    /// <summary>
    ///     Creates an <see cref="ActivityEntry" /> from a <see cref="ProjectDevlog" /> and its associated
    ///     <see cref="Project" />.
    /// </summary>
    /// <param name="devlog">The <see cref="ProjectDevlog" />.</param>
    /// <param name="project">The associated <see cref="Project" />.</param>
    /// <returns>An <see cref="ActivityEntry" /> representing the <paramref name="devlog" />.</returns>
    public static ActivityEntry From(ProjectDevlog devlog, Project project)
    {
        return new ActivityEntry
        {
            CreatedAt = devlog.Published,
            Title = devlog.Title,
            CommitSha = devlog.Id.ToCommitSha(),
            PagePath = "/Projects/Devlog",
            Category = "devlog",
            RouteValues = new Dictionary<string, string> { ["projectSlug"] = project.Slug, ["slug"] = devlog.Slug },
            ReadingMinutes = Option.Some(devlog.GetEstimatedReadingTime()),
            Visibility = devlog.Visibility
        };
    }

    /// <summary>
    ///     Creates an <see cref="ActivityEntry" /> from a <see cref="DevChallenge" />.
    /// </summary>
    /// <param name="challenge">The <see cref="DevChallenge" />.</param>
    /// <returns>An <see cref="ActivityEntry" /> representing the <paramref name="challenge" />.</returns>
    public static ActivityEntry From(DevChallenge challenge)
    {
        return new ActivityEntry
        {
            CreatedAt = challenge.Date,
            Title = challenge.Title,
            CommitSha = challenge.Id.ToCommitSha(),
            PagePath = "/Learn/Challenges/Challenge",
            Category = "challenge",
            RawUrl = Option.Some($"/challenge/{challenge.Id}"),
            Visibility = challenge.Visibility
        };
    }

    /// <summary>
    ///     Creates an <see cref="ActivityEntry" /> from a <see cref="Note" />.
    /// </summary>
    /// <param name="note">The <see cref="Note" />.</param>
    /// <returns>An <see cref="ActivityEntry" /> representing the <paramref name="note" />.</returns>
    public static ActivityEntry From(Note note)
    {
        return new ActivityEntry
        {
            CreatedAt = note.Published,
            Title = note.Title,
            CommitSha = note.Id.ToCommitSha(),
            PagePath = "/Learn/Challenges/Note",
            Category = "note",
            RawUrl = Option.Some($"/note/{(ShortGuid)note.Id}"),
            Visibility = note.Visibility
        };
    }
}
