using Microsoft.AspNetCore.Mvc.RazorPages;
using OliverBooth.Data.Web;
using OliverBooth.Services;

namespace OliverBooth.Pages;

public class Books : PageModel
{
    private readonly IReadingListService _readingListService;

    public Books(IReadingListService readingListService)
    {
        _readingListService = readingListService;
    }

    /// <summary>
    ///     Gets the books currently being read.
    /// </summary>
    /// <value>The books currently being read.</value>
    public IReadOnlyCollection<IBook> CurrentlyReading { get; private set; } = ArraySegment<IBook>.Empty;

    /// <summary>
    ///     Gets the books that are planned to be read.
    /// </summary>
    /// <value>The books that are planned to be read.</value>
    public IReadOnlyCollection<IBook> PlanToRead { get; private set; } = ArraySegment<IBook>.Empty;

    /// <summary>
    ///     Gets the books that have been read.
    /// </summary>
    /// <value>The books that have been read.</value>
    public IReadOnlyCollection<IBook> Read { get; private set; } = ArraySegment<IBook>.Empty;

    public void OnGet()
    {
        CurrentlyReading = _readingListService.GetBooks(BookState.Reading);
        PlanToRead = _readingListService.GetBooks(BookState.PlanToRead);
        Read = _readingListService.GetBooks(BookState.Read);
    }
}
