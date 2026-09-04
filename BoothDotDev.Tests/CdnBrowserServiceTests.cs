using BoothDotDev.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace BoothDotDev.Tests;

[TestFixture]
internal sealed class CdnBrowserServiceTests
{
    [SetUp]
    public void SetUp()
    {
        _root = Path.Combine(Path.GetTempPath(), $"cdn-browser-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_root);
        _service = new CdnBrowserService(NullLogger<CdnBrowserService>.Instance, _root, "https://cdn.test");
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }
    }

    private string _root = null!;
    private CdnBrowserService _service = null!;

    [Test]
    public void Move_FolderIntoItself_Fails()
    {
        Directory.CreateDirectory(Path.Combine(_root, "folder"));

        var result = _service.Move(null, "folder", "folder");

        Assert.That(result.IsFailed, Is.True);
    }

    [Test]
    public void Move_FolderIntoOwnSubfolder_Fails()
    {
        Directory.CreateDirectory(Path.Combine(_root, "folder", "child"));

        var result = _service.Move(null, "folder", "folder/child");

        Assert.That(result.IsFailed, Is.True);
    }

    [Test]
    public void Move_FolderIntoUnrelatedFolder_Succeeds()
    {
        Directory.CreateDirectory(Path.Combine(_root, "source"));
        Directory.CreateDirectory(Path.Combine(_root, "destination"));

        var result = _service.Move(null, "source", "destination");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(Directory.Exists(Path.Combine(_root, "destination", "source")), Is.True);
            Assert.That(Directory.Exists(Path.Combine(_root, "source")), Is.False);
        }
    }

    [Test]
    public void Move_FileWithSimilarlyNamedSiblingFolder_DoesNotFalsePositiveOnSelfMove()
    {
        // "folder" and "folder-2" share a string prefix - the self/descendant guard must compare on a full
        // path-segment boundary, not raw StartsWith, or moving "folder-2" into "folder" would be wrongly rejected.
        Directory.CreateDirectory(Path.Combine(_root, "folder"));
        Directory.CreateDirectory(Path.Combine(_root, "folder-2"));

        var result = _service.Move(null, "folder-2", "folder");

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void PreviewDelete_EmptyFolder_ReportsZero()
    {
        Directory.CreateDirectory(Path.Combine(_root, "empty"));

        var result = _service.PreviewDelete(null, "empty");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.ItemCount, Is.EqualTo(0));
            Assert.That(result.Value.Capped, Is.False);
        }
    }

    [Test]
    public void PreviewDelete_FolderUnderCap_ReportsExactCount()
    {
        var folder = Path.Combine(_root, "folder");
        Directory.CreateDirectory(folder);
        for (var i = 0; i < 3; i++)
        {
            File.WriteAllText(Path.Combine(folder, $"file{i}.txt"), "");
        }

        var result = _service.PreviewDelete(null, "folder", 500);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.ItemCount, Is.EqualTo(3));
            Assert.That(result.Value.Capped, Is.False);
        }
    }

    [Test]
    public void PreviewDelete_FolderOverCap_ReportsCappedCount()
    {
        var folder = Path.Combine(_root, "folder");
        Directory.CreateDirectory(folder);
        for (var i = 0; i < 5; i++)
        {
            File.WriteAllText(Path.Combine(folder, $"file{i}.txt"), "");
        }

        var result = _service.PreviewDelete(null, "folder", 2);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.ItemCount, Is.EqualTo(2));
            Assert.That(result.Value.Capped, Is.True);
        }
    }

    [Test]
    public void PreviewDelete_File_ReportsZero()
    {
        File.WriteAllText(Path.Combine(_root, "file.txt"), "");

        var result = _service.PreviewDelete(null, "file.txt");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.ItemCount, Is.EqualTo(0));
        }
    }

    [Test]
    public void Delete_NonEmptyFolder_RemovesEverything()
    {
        var folder = Path.Combine(_root, "folder");
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "file.txt"), "");

        var result = _service.Delete(null, "folder");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(Directory.Exists(folder), Is.False);
        }
    }

    [Test]
    public void CreateFolder_Then_ListDirectory_ShowsIt()
    {
        _service.CreateFolder(null, "new-folder");

        var listing = _service.ListDirectory(null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(listing.IsSuccess, Is.True);
            Assert.That(listing.Value.Entries, Has.Count.EqualTo(1));
            Assert.That(listing.Value.Entries[0].Name, Is.EqualTo("new-folder"));
            Assert.That(listing.Value.Entries[0].IsDirectory, Is.True);
        }
    }

    [Test]
    public void ListDirectory_NonexistentPath_Fails()
    {
        var result = _service.ListDirectory("does/not/exist");

        Assert.That(result.IsFailed, Is.True);
    }
}
