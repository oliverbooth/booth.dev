using System.Security.Cryptography;
using System.Text;
using BoothDotDev.Data;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BoothDotDev.Controllers;

/// <summary>
///     Represents the phone status ingestion controller, for the phone-side automation backing the /now page's
///     battery hook.
/// </summary>
[ApiController]
[Route("api/phone-status")]
[Produces("application/json")]
public sealed class PhoneStatusController : ControllerBase
{
    private readonly IOptionsMonitor<PhoneStatusOptions> _options;
    private readonly PhoneStatusService _phoneStatusService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PhoneStatusController" /> class.
    /// </summary>
    /// <param name="phoneStatusService">The phone status service.</param>
    /// <param name="options">The phone status options.</param>
    public PhoneStatusController(PhoneStatusService phoneStatusService, IOptionsMonitor<PhoneStatusOptions> options)
    {
        _phoneStatusService = phoneStatusService;
        _options = options;
    }

    /// <summary>
    ///     Records a phone status report.
    /// </summary>
    /// <param name="report">The reported status.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    [HttpPost]
    public IActionResult Report([FromBody] PhoneStatusReport report)
    {
        var expectedSecret = _options.CurrentValue.Secret;
        var providedSecret = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        if (string.IsNullOrEmpty(expectedSecret) || !SecretsMatch(providedSecret, expectedSecret))
        {
            return Unauthorized();
        }

        if (report.BatteryLevel is < 0 or > 100)
        {
            return BadRequest("batteryLevel must be between 0 and 100.");
        }

        _phoneStatusService.Report(report.BatteryLevel, report.IsCharging);
        return NoContent();
    }

    /// <summary>
    ///     Compares two secrets in constant time, so a mismatch can't be timed to learn anything about the real one.
    /// </summary>
    /// <param name="provided">The secret provided by the caller.</param>
    /// <param name="expected">The configured secret.</param>
    /// <returns><see langword="true" /> if the secrets match; otherwise, <see langword="false" />.</returns>
    private static bool SecretsMatch(string provided, string expected)
    {
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        return providedBytes.Length == expectedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }
}

/// <summary>
///     Represents a phone status report from the phone-side automation.
/// </summary>
/// <param name="BatteryLevel">The battery level, from 0 to 100.</param>
/// <param name="IsCharging">Whether the phone is currently charging.</param>
public sealed record PhoneStatusReport(int BatteryLevel, bool IsCharging);
