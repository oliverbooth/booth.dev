using BoothDotDev.CodeBlockTrivia.Highlighting;

namespace BoothDotDev.Tests;

[TestFixture]
internal sealed class SpecRangeTests
{
    private const string HelloWorld = "Hello World";

    [Test]
    public void Forward2ToBack2_HighlightsMiddleOnly()
    {
        // spec: L1@2..^2  =>  "ello Worl" (drop first char 'H' and last char 'd')
        var range = new SpecRange(SpecBound.Forward(2), SpecBound.Backward(2));

        var result = HelloWorld[range.ToRange()];

        Assert.That(result, Is.EqualTo("ello Worl"));
    }

    [Test]
    public void Forward1ToBack1_HighlightsEntireLine()
    {
        // spec: L1@1..^1  =>  identity case, whole line, drop nothing
        var range = new SpecRange(SpecBound.Forward(1), SpecBound.Backward(1));

        var result = HelloWorld[range.ToRange()];

        Assert.That(result, Is.EqualTo(HelloWorld));
    }

    [Test]
    public void Forward1ToBack2_DropsOnlyTrailingChar()
    {
        // spec: L1@1..^2  =>  "Hello Worl" (drop only the trailing 'd')
        var range = new SpecRange(SpecBound.Forward(1), SpecBound.Backward(2));

        var result = HelloWorld[range.ToRange()];

        Assert.That(result, Is.EqualTo("Hello Worl"));
    }

    [Test]
    public void ForwardOnlyRange_2To8_MatchesPlainColumnSlice()
    {
        // spec: L1@2..8  =>  "ello Wo" (no ^ involved at all)
        var range = new SpecRange(SpecBound.Forward(2), SpecBound.Forward(8));

        var result = HelloWorld[range.ToRange()];

        Assert.That(result, Is.EqualTo("ello Wo"));
    }

    [Test]
    public void BackToBack_5To2_MatchesExpectedSpan()
    {
        // spec: L1@^5..^2  =>  "Worl" (5th-from-end 'W' through 2nd-from-end 'l', i.e. "World" minus the trailing 'd')
        var range = new SpecRange(SpecBound.Backward(5), SpecBound.Backward(2));

        var result = HelloWorld[range.ToRange()];

        Assert.That(result, Is.EqualTo("Worl"));
    }

    [TestCase(1, false, 0)] // forward start: -1
    [TestCase(2, false, 1)]
    public void ToStartIndex_Forward_SubtractsOne(int specValue, bool isFromEnd, int expectedIndexValue)
    {
        var bound = new SpecBound(specValue, isFromEnd);

        var index = bound.ToStartIndex();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(index.Value, Is.EqualTo(expectedIndexValue));
            Assert.That(index.IsFromEnd, Is.False);
        }
    }

    [TestCase(1, true, 1)] // back start: unchanged
    [TestCase(5, true, 5)]
    public void ToStartIndex_Backward_IsUnchanged(int specValue, bool isFromEnd, int expectedIndexValue)
    {
        var bound = new SpecBound(specValue, isFromEnd);

        var index = bound.ToStartIndex();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(index.Value, Is.EqualTo(expectedIndexValue));
            Assert.That(index.IsFromEnd, Is.True);
        }
    }

    [TestCase(8, false, 8)] // forward end: unchanged
    [TestCase(1, false, 1)]
    public void ToEndIndex_Forward_IsUnchanged(int specValue, bool isFromEnd, int expectedIndexValue)
    {
        var bound = new SpecBound(specValue, isFromEnd);

        var index = bound.ToEndIndex();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(index.Value, Is.EqualTo(expectedIndexValue));
            Assert.That(index.IsFromEnd, Is.False);
        }
    }

    [TestCase(2, true, 1)] // back end: -1
    [TestCase(1, true, 0)]
    public void ToEndIndex_Backward_SubtractsOne(int specValue, bool isFromEnd, int expectedIndexValue)
    {
        var bound = new SpecBound(specValue, isFromEnd);

        var index = bound.ToEndIndex();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(index.Value, Is.EqualTo(expectedIndexValue));
            Assert.That(index.IsFromEnd, Is.True);
        }
    }
}
