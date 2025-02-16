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

    [HttpGet("status/{repo}/{workflow}")]
    [HttpGet("status/{owner}/{repo}/{workflow}")]
    public async Task<IActionResult> StatusAsync(string repo, string workflow, string owner = "oliverbooth")
    {
        string? githubToken = _configuration.GetSection("GitHub:Token").Value;

        var url = $"https://api.github.com/repos/{owner}/{repo}/actions/workflows/{workflow}/runs";
        Console.WriteLine(url);
        using HttpClient client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage();
        request.RequestUri = new Uri(url);
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Authorization", $"Bearer {githubToken}");
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("booth.dev", _version));

        using HttpResponseMessage response = await client.SendAsync(request);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        var body = await response.Content.ReadFromJsonAsync<WorkflowRunSchema>();
        WorkflowRun? run = body?.WorkflowRuns.FirstOrDefault(r => r.Status == WorkflowRunStatus.Completed);
        if (run is not null)
        {
            var color = run.Conclusion switch
            {
                WorkflowRunConclusion.Failure => "e05d44",
                WorkflowRunConclusion.Success => "44cc11",
                _ => "lightgray"
            };
            var message = run.Conclusion switch
            {
                WorkflowRunConclusion.Failure => "failing",
                WorkflowRunConclusion.Success => "passing",
                _ => "unknown"
            };

            return Ok(new { schemaVersion = 1, label = "build", color, message });
        }

        return Ok(new { schemaVersion = 1, label = "build", color = "lightgray", message = "unknown" });
    }

    private class WorkflowRunSchema
    {
        [JsonPropertyName("workflow_runs"), JsonInclude]
        public WorkflowRun[] WorkflowRuns { get; set; } = Array.Empty<WorkflowRun>();
    }

    private class WorkflowRun
    {
        [JsonPropertyName("conclusion"), JsonInclude]
        [JsonConverter(typeof(JsonStringEnumConverter<WorkflowRunConclusion>))]
        public WorkflowRunConclusion Conclusion { get; set; } = WorkflowRunConclusion.Unknown;

        [JsonPropertyName("status"), JsonInclude]
        [JsonConverter(typeof(JsonStringEnumConverter<WorkflowRunStatus>))]
        public WorkflowRunStatus Status { get; set; } = WorkflowRunStatus.Unknown;
    }

    private enum WorkflowRunStatus
    {
        Unknown = -1,
        Completed
    }

    private enum WorkflowRunConclusion
    {
        Unknown = -1,
        Success,
        Failure
    }
}
