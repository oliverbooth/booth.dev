using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OliverBooth.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    private readonly ILogger<ErrorModel> _logger;

    public ErrorModel(ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }

    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public int StatusCode { get; private set; }

    public void OnGet(int? code = null)
    {
        StatusCode = code ?? HttpContext.Response.StatusCode;
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        var originalPath = "unknown";
        if (HttpContext.Items.TryGetValue("originalPath", out object? value))
        {
            originalPath = value as string;
        }
    }
}
