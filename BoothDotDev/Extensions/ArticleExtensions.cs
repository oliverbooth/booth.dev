using System.Buffers;
using System.Text;
using BoothDotDev.Data.Models;
using Markdig;
using Markdig.Syntax;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extension methods for <see cref="BlogPost" />, <see cref="ProjectDevlog" />, and <see cref="TutorialArticle" />.
/// </summary>
internal static class ArticleExtensions
{
    private const int WordsPerMinute = 275;

    private static readonly MarkdownPipeline WordCountPipeline = new MarkdownPipelineBuilder().Build();

    private static readonly SearchValues<char> WhitespaceChars = SearchValues.Create(
        "\t\n\v\f\r " +
        "\u0085\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u2028\u2029\u202F\u205F\u3000"
    );

    /// <param name="post">The <see cref="BlogPost" />.</param>
    extension(BlogPost post)
    {
        /// <summary>
        ///     Returns the estimated reading time of the blog post, in minutes.
        /// </summary>
        /// <returns>The estimated reading time of the blog post, in minutes.</returns>
        public int GetEstimatedReadingTime()
        {
            var wordCount = CountWords(post.Body);
            return Math.Max(1, wordCount / WordsPerMinute);
        }
    }

    /// <param name="devlog">The <see cref="ProjectDevlog" />.</param>
    extension(ProjectDevlog devlog)
    {
        /// <summary>
        ///     Returns the estimated reading time of the devlog entry, in minutes.
        /// </summary>
        /// <returns>The estimated reading time of the devlog entry, in minutes.</returns>
        public int GetEstimatedReadingTime()
        {
            var wordCount = CountWords(devlog.Body);
            return Math.Max(1, wordCount / WordsPerMinute);
        }
    }

    /// <param name="article">The <see cref="TutorialArticle" />.</param>
    extension(TutorialArticle article)
    {
        /// <summary>
        ///     Returns the estimated reading time of the tutorial article, in minutes.
        /// </summary>
        /// <returns>The estimated reading time of the tutorial article, in minutes.</returns>
        public int GetEstimatedReadingTime()
        {
            var wordCount = CountWords(article.Body);
            return Math.Max(1, wordCount / WordsPerMinute);
        }
    }

    /// <summary>
    ///     Counts words in <paramref name="body" />, excluding the content of manim-2d/manim-3d/vexflow fenced codeblocks.
    /// </summary>
    /// <param name="body">The body text in which to count words.</param>
    private static int CountWords(string body)
    {
        MarkdownDocument document = global::Markdig.Markdown.Parse(body, WordCountPipeline);
        return CountWords(ExcludeSceneCodeBlocks(body, document).AsSpan());
    }

    /// <summary>
    ///     Returns <paramref name="body" /> with the source text of every scene fenced code block cut out.
    /// </summary>
    /// <param name="body">The original body text.</param>
    /// <param name="document">The parsed Markdown document.</param>
    private static string ExcludeSceneCodeBlocks(string body, MarkdownDocument document)
    {
        FencedCodeBlock[] sceneBlocks =
        [
            .. document.Descendants()
                .OfType<FencedCodeBlock>()
                .Where(IsSceneCodeBlock)
                .OrderBy(block => block.Span.Start)
        ];

        if (sceneBlocks.Length == 0)
        {
            return body;
        }

        var builder = new StringBuilder(body.Length);
        var cursor = 0;

        foreach (FencedCodeBlock block in sceneBlocks)
        {
            if (block.Span.Start > cursor)
            {
                builder.Append(body, cursor, block.Span.Start - cursor);
            }

            cursor = Math.Max(cursor, block.Span.End + 1); // Span.End is inclusive
        }

        if (cursor < body.Length)
        {
            builder.Append(body, cursor, body.Length - cursor);
        }

        return builder.ToString();
    }

    private static bool IsSceneCodeBlock(FencedCodeBlock block)
    {
        if (string.IsNullOrEmpty(block.Arguments))
        {
            return false;
        }

        var arguments = block.Arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return arguments.Contains("manim-2d") || arguments.Contains("manim-3d") || arguments.Contains("vexflow");
    }

    private static int CountWords(ReadOnlySpan<char> text)
    {
        var wordCount = 0;

        while (!text.IsEmpty)
        {
            var nonWhitespaceStart = text.IndexOfAnyExcept(WhitespaceChars);
            if (nonWhitespaceStart < 0)
            {
                break; // rest is all whitespace
            }

            text = text[nonWhitespaceStart..];
            wordCount++;

            var nextWhitespace = text.IndexOfAny(WhitespaceChars);
            text = nextWhitespace < 0 ? [] : text[nextWhitespace..];
        }

        return wordCount;
    }
}
