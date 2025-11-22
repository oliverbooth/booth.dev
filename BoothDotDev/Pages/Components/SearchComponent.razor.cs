using BoothDotDev.Common.Data;
using BoothDotDev.Common.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BoothDotDev.Pages.Components;

/// <summary>
///     A component for searching content.
/// </summary>
public partial class SearchComponent : ComponentBase, IDisposable
{
    private readonly ISearchService _searchService;
    private CancellationTokenSource? _debounceCts;
    private readonly TimeSpan _debounceDelay = TimeSpan.FromMilliseconds(300);

    /// <summary>
    ///     Initializes a new instance of the <see cref="SearchComponent" /> class.
    /// </summary>
    /// <param name="searchService">The <see cref="ISearchService" />.</param>
    public SearchComponent(ISearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    ///     Gets or sets the search results.
    /// </summary>
    /// <value>The search results.</value>
    public IReadOnlyCollection<SearchResult> SearchResults { get; set; } = [];

    /// <summary>
    ///     Gets or sets the search text.
    /// </summary>
    /// <value>The search text.</value>
    public string SearchText
    {
        get => field;
        set
        {
            if (value == field) return;
            field = value;
            _ = DebouncedSearchAsync();
        }
    } = string.Empty;

    private async Task DebouncedSearchAsync()
    {
        if (_debounceCts is not null)
        {
            await _debounceCts.CancelAsync();
            _debounceCts.Dispose();
        }

        _debounceCts = new CancellationTokenSource();
        CancellationToken token = _debounceCts.Token;

        try
        {
            await Task.Delay(_debounceDelay, token);
            if (!token.IsCancellationRequested)
            {
                await DoSearchAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // expected on cancellation, ignore
        }
    }

    private async Task DoSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            SearchResults = [];
        }
        else
        {
            IReadOnlyCollection<SearchResult> results = await _searchService.SearchAsync(SearchText);
            SearchResults = results;
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task OnKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            if (_debounceCts is not null)
            {
                await _debounceCts.CancelAsync();
            }

            await DoSearchAsync();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
