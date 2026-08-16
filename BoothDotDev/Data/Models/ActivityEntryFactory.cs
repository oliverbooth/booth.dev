using BoothDotDev.Extensions;
using BoothDotDev.Services;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Provides factory methods for creating <see cref="ActivityEntry" /> instances from various data models.
/// </summary>
public static class ActivityEntryFactory
{
    /// <summary>
    ///     Creates an <see cref="ActivityEntry.Blog" /> from a <see cref="BlogPost" />.
    /// </summary>
    /// <param name="post">The <see cref="BlogPost" />.</param>
    /// <returns>An <see cref="ActivityEntry.Blog" /> representing the <paramref name="post" />.</returns>
    public static ActivityEntry.Blog From(BlogPost post)
    {
        return new(post.Published,
            post.Title,
            "blog",
            new() { ["slug"] = post.Slug },
            post.GetEstimatedReadingTime(),
            post.Id.ToString("N")[..7]);
    }

    /// <summary>
    ///     Creates an <see cref="ActivityEntry.Tutorial" /> from a <see cref="TutorialArticle" />.
    /// </summary>
    /// <param name="article">The <see cref="TutorialArticle" />.</param>
    /// <param name="tutorialService">The <see cref="TutorialService" />.</param>
    /// <returns>An <see cref="ActivityEntry.Tutorial" /> representing the <paramref name="article" />.</returns>
    public static ActivityEntry.Tutorial From(TutorialArticle article, TutorialService tutorialService)
    {
        return new(article.Published,
            article.Title,
            "tutorial",
            new() { ["slug"] = tutorialService.GetFullSlug(article) },
            article.GetEstimatedReadingTime(),
            article.Id.ToString("N")[..7]);
    }

    /// <summary>
    ///     Creates an <see cref="ActivityEntry.Devlog" /> from a <see cref="ProjectDevlog" /> and its associated
    ///     <see cref="Project" />.
    /// </summary>
    /// <param name="devlog">The <see cref="ProjectDevlog" />.</param>
    /// <param name="project">The associated <see cref="Project" />.</param>
    /// <returns>An <see cref="ActivityEntry.Devlog" /> representing the <paramref name="devlog" />.</returns>
    public static ActivityEntry.Devlog From(ProjectDevlog devlog, Project project)
    {
        return new(devlog.Published,
            devlog.Title,
            "devlog",
            new() { ["projectSlug"] = project.Slug, ["slug"] = devlog.Slug },
            devlog.GetEstimatedReadingTime(),
            devlog.Id.ToString("N")[..7]);
    }

    /// <summary>
    ///     Creates an <see cref="ActivityEntry.Challenge" /> from a <see cref="DevChallenge" />.
    /// </summary>
    /// <param name="challenge">The <see cref="DevChallenge" />.</param>
    /// <returns>An <see cref="ActivityEntry.Challenge" /> representing the <paramref name="challenge" />.</returns>
    public static ActivityEntry.Challenge From(DevChallenge challenge)
    {
        return new(challenge.Date,
            challenge.Title,
            "challenge",
            $"/challenge/{challenge.Id}",
            ((Guid)challenge.Id).ToString("N")[..7]);
    }
}
