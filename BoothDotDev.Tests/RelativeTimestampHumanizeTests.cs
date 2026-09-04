using System.Text.Json;
using Humanizer;

namespace BoothDotDev.Tests;

/// <summary>
///     Verifies <c>DateTimeOffset.Humanize()</c> - as used by <c>TimestampRenderer</c> for relative timestamps - against a
///     fixture shared with the client-side port in <c>utils.ts</c> (see its <c>relative-timestamp.test.ts</c>).
/// </summary>
[TestFixture]
internal sealed class RelativeTimestampHumanizeTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly RelativeTimestampFixture Fixture = LoadFixture();

    private static IEnumerable<TestCaseData> TestCases()
    {
        var reference = new DateTimeOffset(Fixture.ReferenceUtc, TimeSpan.Zero);

        for (var index = 0; index < Fixture.Cases.Length; index++)
        {
            var testCase = Fixture.Cases[index];
            var target = new DateTimeOffset(testCase.TargetUtc, TimeSpan.Zero);
            yield return new TestCaseData(target, reference, testCase.Expected)
                .SetName($"Humanize_Case{index:00}_{Sanitize(testCase.Expected)}");
        }
    }

    [TestCaseSource(nameof(TestCases))]
    public void Humanize_MatchesFixture(DateTimeOffset target, DateTimeOffset reference, string expected)
    {
        var actual = target.Humanize(reference);

        Assert.That(actual, Is.EqualTo(expected));
    }

    private static string Sanitize(string value)
    {
        return value.Replace(' ', '_');
    }

    private static RelativeTimestampFixture LoadFixture()
    {
        var json = File.ReadAllText(ResolveFixturePath());
        return JsonSerializer.Deserialize<RelativeTimestampFixture>(json, JsonOptions)
               ?? throw new InvalidOperationException("Failed to load the relative-timestamp fixture.");
    }

    private static string ResolveFixturePath()
    {
        // AppContext.BaseDirectory is always <project>/bin/<config>/<tfm>/ - four levels up is the repo root
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        return Path.Combine(repoRoot, "test-fixtures", "relative-timestamp.json");
    }

    private sealed record RelativeTimestampFixture(DateTime ReferenceUtc, RelativeTimestampCase[] Cases);

    private sealed record RelativeTimestampCase(DateTime TargetUtc, string Expected);
}
