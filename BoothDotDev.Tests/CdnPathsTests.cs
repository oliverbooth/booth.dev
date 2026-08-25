using BoothDotDev.Services;

namespace BoothDotDev.Tests;

[TestFixture]
internal sealed class CdnPathsTests
{
    private const string Root = "/fake/root";

    [TestCase(null)]
    [TestCase("")]
    public void ResolveRelative_NullOrEmpty_ResolvesToRoot(string? input)
    {
        var result = CdnPaths.ResolveRelative(Root, input);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.AbsolutePath, Is.EqualTo(Root));
            Assert.That(result.Value.DisplayPath, Is.EqualTo("/"));
            Assert.That(result.Value.Segments, Is.Empty);
        }
    }

    [TestCase("foo/bar")]
    [TestCase("/foo/bar")]
    [TestCase("foo//bar")]
    [TestCase("/foo/bar/")]
    public void ResolveRelative_EquivalentForms_ResolveIdentically(string input)
    {
        var result = CdnPaths.ResolveRelative(Root, input);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.AbsolutePath, Is.EqualTo(Path.Combine(Root, "foo", "bar")));
            Assert.That(result.Value.DisplayPath, Is.EqualTo("/foo/bar"));
            Assert.That(result.Value.Segments, Is.EqualTo(new[] { "foo", "bar" }));
        }
    }

    [TestCase("../../etc/passwd")]
    [TestCase("foo/../bar")]
    [TestCase("foo/..")]
    [TestCase("..")]
    [TestCase(".")]
    [TestCase("foo/.")]
    public void ResolveRelative_TraversalAttempt_Fails(string input)
    {
        var result = CdnPaths.ResolveRelative(Root, input);

        Assert.That(result.IsFailed, Is.True);
    }

    [TestCase("foo\\bar")]
    [TestCase("..\\..\\etc")]
    public void ResolveRelative_Backslash_Fails(string input)
    {
        var result = CdnPaths.ResolveRelative(Root, input);

        Assert.That(result.IsFailed, Is.True);
    }

    [TestCase("foo bar")]
    [TestCase("foo;bar")]
    [TestCase("foo$bar")]
    [TestCase("foo/bar!")]
    public void ResolveRelative_DisallowedCharacters_Fails(string input)
    {
        var result = CdnPaths.ResolveRelative(Root, input);

        Assert.That(result.IsFailed, Is.True);
    }

    [Test]
    public void ResolveRelative_SegmentOver120Characters_Fails()
    {
        var result = CdnPaths.ResolveRelative(Root, new string('a', 121));

        Assert.That(result.IsFailed, Is.True);
    }

    [Test]
    public void ResolveRelative_Segment120Characters_Succeeds()
    {
        var result = CdnPaths.ResolveRelative(Root, new string('a', 120));

        Assert.That(result.IsSuccess, Is.True);
    }

    [TestCase("a")]
    [TestCase("file.txt")]
    [TestCase("my-file_v2.final.png")]
    public void ValidateSegment_ValidNames_Succeed(string name)
    {
        var result = CdnNaming.ValidateSegment(name);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(name));
        }
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(".hidden")]
    [TestCase("-leading-dash")]
    [TestCase("has space")]
    [TestCase("has/slash")]
    public void ValidateSegment_InvalidNames_Fail(string? name)
    {
        var result = CdnNaming.ValidateSegment(name);

        Assert.That(result.IsFailed, Is.True);
    }
}
