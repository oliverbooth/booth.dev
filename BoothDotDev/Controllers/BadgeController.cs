using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace BoothDotDev.Controllers;

[ApiController]
[Route("api/badge")]
[Produces("application/json")]
public sealed class BadgeController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _version;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BadgeController" /> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public BadgeController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;

        var attribute = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        _version = attribute?.InformationalVersion ?? "1.0.0";
    }

    [HttpGet("github/{repo}/{workflow}")]
    [HttpGet("github/{owner}/{repo}/{workflow}")]
    public async Task<IActionResult> GitHubStatusAsync(string repo, string workflow, string owner = "oliverbooth")
    {
        string githubToken;
        if (Request.Headers.Authorization.Count == 0)
        {
            githubToken = _configuration.GetSection("GitHub:Token").Value ?? string.Empty;
        }
        else
        {
            githubToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        }

        if (string.IsNullOrEmpty(githubToken))
        {
            return StatusCode(500, new { schemaVersion = 1, label = "build", color = "lightgray", message = "no token" });
        }

        var url = $"https://api.github.com/repos/{owner}/{repo}/actions/workflows/{workflow}/runs";

        using HttpClient client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage();
        request.RequestUri = new Uri(url);
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Authorization", $"Bearer {githubToken}");
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("booth.dev", _version));

        using HttpResponseMessage response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, new { schemaVersion = 1, label = "build", color = "lightgray", message = "error" });
        }

        WorkflowRunSchema? body = await response.Content.ReadFromJsonAsync<WorkflowRunSchema>();
        WorkflowRun? run = body?.WorkflowRuns.FirstOrDefault(r => r.Status == WorkflowRunStatus.Completed);
        if (run is null)
        {
            return Ok(new { schemaVersion = 1, label = "build", color = "lightgray", message = "unknown" });
        }

        (string message, string color, bool isError) = run.Conclusion switch
        {
            WorkflowRunConclusion.Failure => ("failing", "red", true),
            WorkflowRunConclusion.Success => ("passing", "brightgreen", false),
            WorkflowRunConclusion.Cancelled or WorkflowRunConclusion.Neutral => ("neutral", "yellow", false),
            _ => ("unknown", "lightgrey", false)
        };

        return Ok(new { schemaVersion = 1, label = "build", color, message, isError });
    }
}

file class WorkflowRunSchema
{
    [JsonPropertyName("workflow_runs"), JsonInclude]
    public WorkflowRun[] WorkflowRuns { get; set; } = [];
}

file class WorkflowRun
{
    [JsonPropertyName("conclusion"), JsonInclude]
    [JsonConverter(typeof(JsonStringEnumConverter<WorkflowRunConclusion>))]
    public WorkflowRunConclusion Conclusion { get; set; } = WorkflowRunConclusion.Unknown;

    [JsonPropertyName("status"), JsonInclude]
    [JsonConverter(typeof(JsonStringEnumConverter<WorkflowRunStatus>))]
    public WorkflowRunStatus Status { get; set; } = WorkflowRunStatus.Unknown;
}

file enum WorkflowRunStatus
{
    Unknown = -1,
    Completed
}

file enum WorkflowRunConclusion
{
    Unknown = -1,
    Success,
    Failure,
    Cancelled,
    Neutral
}
