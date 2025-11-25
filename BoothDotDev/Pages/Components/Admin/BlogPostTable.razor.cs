using BoothDotDev.Common.Data.Blog;
using BoothDotDev.Common.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BoothDotDev.Pages.Components.Admin;

public partial class BlogPostTable : ComponentBase
{
    private readonly IBlogPostService _blogPostService;
    private readonly IJSRuntime _jsRuntime;
    private IReadOnlyList<IBlogPost> _blogPosts = [];
    private IReadOnlyList<IBlogPost> _displayedPosts = [];
    private string _searchText = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogPostTable" /> class.
    /// </summary>
    /// <param name="blogPostService">The blog post service.</param>
    /// <param name="jsRuntime">The JavaScript runtime.</param>
    public BlogPostTable(IBlogPostService blogPostService, IJSRuntime jsRuntime)
    {
        _blogPostService = blogPostService;
        _jsRuntime = jsRuntime;
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await _jsRuntime.InvokeVoidAsync("lucide.createIcons");
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _displayedPosts = _blogPosts = _blogPostService.GetEverySingleBlogPost();
    }

    private void DoSearch()
    {
        const StringComparison comparison = StringComparison.OrdinalIgnoreCase;

        if (string.IsNullOrWhiteSpace(_searchText))
        {
            _displayedPosts = _blogPosts;
            return;
        }

        ReadOnlySpan<char> s = _searchText.AsSpan();
        int sLen = s.Length;
        var results = new List<IBlogPost>(_blogPosts.Count);

        foreach (IBlogPost post in _blogPosts)
        {
            ReadOnlySpan<char> titleSpan = post.Title.AsSpan();
            var allFound = true;
            var i = 0;

            while (i < sLen)
            {
                // skip spaces
                while (i < sLen && s[i] == ' ') i++;
                if (i >= sLen)
                {
                    break;
                }

                int start = i;
                while (i < sLen && s[i] != ' ') i++;
                ReadOnlySpan<char> token = s.Slice(start, i - start);
                if (token.Length == 0) continue;

                if (titleSpan.IndexOf(token, comparison) < 0)
                {
                    allFound = false;
                    break;
                }
            }

            if (allFound)
            {
                results.Add(post);
            }
        }

        _displayedPosts = results;
    }

    private void OnKeyUp(KeyboardEventArgs e)
    {
        DoSearch();
    }
}
