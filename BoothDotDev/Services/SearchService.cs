using BoothDotDev.Common.Data;
using BoothDotDev.Common.Services;

namespace BoothDotDev.Services;

/// <inheritdoc />
internal sealed class SearchService : ISearchService
{
    private readonly IBlogPostService _blogPostService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SearchService" /> class.
    /// </summary>
    /// <param name="blogPostService">The blog post service.</param>
    public SearchService(IBlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<SearchResult>> SearchAsync(string searchText)
    {
        var results = new List<SearchResult>();
        results.AddRange((await _blogPostService.SearchBlogPostsAsync(searchText)).Select(post => new SearchResult
        {
            Title = post.Title,
            Url = $"/blog/{post.Published:yyyy/MM/dd}/{post.Slug}"
        }));
        return results;
    }
}
