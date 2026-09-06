using BoothDotDev.Data;
using Microsoft.Extensions.Options;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service which holds the most recently reported phone battery status, for the /now page.
/// </summary>
/// <param name="options">The <see cref="IOptionsMonitor{PhoneStatusOptions}" /> to use for accessing configuration options.</param>
public sealed class PhoneStatusService(IOptionsMonitor<PhoneStatusOptions> options)
{
    private PhoneStatusSnapshot? _snapshot;

    /// <summary>
    ///     Records a new phone status report.
    /// </summary>
    /// <param name="batteryLevel">The battery level, from 0 to 100.</param>
    /// <param name="isCharging">Whether the phone is currently charging.</param>
    public void Report(int batteryLevel, bool isCharging)
    {
        _snapshot = new PhoneStatusSnapshot(batteryLevel, isCharging, DateTimeOffset.UtcNow);
    }

    /// <summary>
    ///     Gets the most recently reported phone status.
    /// </summary>
    /// <returns>The most recent status, or <see langword="null" /> if none has been reported, or it's gone stale.</returns>
    public PhoneStatusSnapshot? GetStatus()
    {
        var snapshot = _snapshot;
        if (snapshot is null)
        {
            return null;
        }

        var staleAfter = TimeSpan.FromMinutes(options.CurrentValue.StaleAfterMinutes);
        return DateTimeOffset.UtcNow - snapshot.ReportedAt > staleAfter ? null : snapshot;
    }
}

/// <summary>
///     Represents a single phone status report.
/// </summary>
/// <param name="BatteryLevel">The battery level, from 0 to 100.</param>
/// <param name="IsCharging">Whether the phone was charging at the time of the report.</param>
/// <param name="ReportedAt">The date and time at which the report was received.</param>
public sealed record PhoneStatusSnapshot(int BatteryLevel, bool IsCharging, DateTimeOffset ReportedAt);
