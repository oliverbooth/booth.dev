using BoothDotDev.CodeBlockTrivia.Highlighting;

namespace BoothDotDev.Tests;

[TestFixture]
internal sealed class TriviaParserTests
{
    [Test]
    public void SingleLine_ParsesCorrectly()
    {
        var success = TriviaParser.TryParse("h=L3", out TriviaBlock block, out TriviaParseError error);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(error, Is.EqualTo(TriviaParseError.None));
            Assert.That(block.Kind, Is.EqualTo("h"));
            Assert.That(block.Tokens, Has.Count.EqualTo(1));
        }

        HighlightToken token = block.Tokens[0];
        Assert.That(token.Lines, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(token.Lines[0].Start, Is.EqualTo(SpecBound.Forward(3)));
            Assert.That(token.Lines[0].IsRange, Is.False);
            Assert.That(token.IsGrouped, Is.False);
            Assert.That(token.Columns, Is.Null);
        }
    }

    [Test]
    public void LineRange_ParsesCorrectly()
    {
        var success = TriviaParser.TryParse("h=L3-L5", out TriviaBlock block, out _);

        Assert.That(success, Is.True);
        HighlightToken token = block.Tokens[0];
        Assert.That(token.Lines, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(token.Lines[0].Start, Is.EqualTo(SpecBound.Forward(3)));
            Assert.That(token.Lines[0].End, Is.EqualTo(SpecBound.Forward(5)));
            Assert.That(token.IsGrouped, Is.False);
        }
    }

    [Test]
    public void MultipleDisjointLineRanges_ParsesAsSeparateTokens()
    {
        var success = TriviaParser.TryParse("h=L1-L3,L5-L7", out TriviaBlock block, out _);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(block.Tokens, Has.Count.EqualTo(2));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(block.Tokens[0].Lines[0].Start, Is.EqualTo(SpecBound.Forward(1)));
            Assert.That(block.Tokens[0].Lines[0].End, Is.EqualTo(SpecBound.Forward(3)));
            Assert.That(block.Tokens[1].Lines[0].Start, Is.EqualTo(SpecBound.Forward(5)));
            Assert.That(block.Tokens[1].Lines[0].End, Is.EqualTo(SpecBound.Forward(7)));
        }
    }

    [Test]
    public void ColumnRange_AttachesToLastLine_WhenUngrouped()
    {
        var success = TriviaParser.TryParse("h=L3-L5@2..8", out TriviaBlock block, out _);

        Assert.That(success, Is.True);
        HighlightToken token = block.Tokens[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(token.IsGrouped, Is.False); // ungrouped: column spec only applies to the last line, per spec
            Assert.That(token.Columns, Has.Count.EqualTo(1));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(token.Columns![0].Start, Is.EqualTo(SpecBound.Forward(2)));
            Assert.That(token.Columns[0].End, Is.EqualTo(SpecBound.Forward(8)));
        }
    }

    [Test]
    public void ColumnRange_AttachesToWholeSpan_WhenGrouped()
    {
        var success = TriviaParser.TryParse("h=(L3-L5)@2..8", out TriviaBlock block, out _);

        Assert.That(success, Is.True);
        HighlightToken token = block.Tokens[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(token.IsGrouped, Is.True);
            Assert.That(token.Lines, Has.Count.EqualTo(1));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(token.Lines[0].Start, Is.EqualTo(SpecBound.Forward(3)));
            Assert.That(token.Lines[0].End, Is.EqualTo(SpecBound.Forward(5)));
        }
    }

    [Test]
    public void DisjointLineSpans_GroupedWithSharedColumns_ParsesAsOneToken()
    {
        var success = TriviaParser.TryParse("h=(L7-L8,L13-L14)@9..^1", out TriviaBlock block, out _);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(block.Tokens, Has.Count.EqualTo(1));
        }

        HighlightToken token = block.Tokens[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(token.IsGrouped, Is.True);
            Assert.That(token.Lines, Has.Count.EqualTo(2));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(token.Lines[0].Start, Is.EqualTo(SpecBound.Forward(7)));
            Assert.That(token.Lines[0].End, Is.EqualTo(SpecBound.Forward(8)));
            Assert.That(token.Lines[1].Start, Is.EqualTo(SpecBound.Forward(13)));
            Assert.That(token.Lines[1].End, Is.EqualTo(SpecBound.Forward(14)));
            Assert.That(token.Columns![0].Start, Is.EqualTo(SpecBound.Forward(9)));
            Assert.That(token.Columns[0].End, Is.EqualTo(SpecBound.Backward(1)));
        }
    }

    [Test]
    public void DisjointSingleLines_GroupedWithSharedColumns_ParsesAsOneToken()
    {
        var success = TriviaParser.TryParse("h=(L3,L9)@2..5", out TriviaBlock block, out _);

        Assert.That(success, Is.True);
        HighlightToken token = block.Tokens[0];
        Assert.That(token.Lines, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(token.Lines[0].IsRange, Is.False);
            Assert.That(token.Lines[0].Start, Is.EqualTo(SpecBound.Forward(3)));
            Assert.That(token.Lines[1].IsRange, Is.False);
            Assert.That(token.Lines[1].Start, Is.EqualTo(SpecBound.Forward(9)));
        }
    }

    [Test]
    public void MultipleColumnRanges_OnSingleLine_ParsesAsGroup()
    {
        var success = TriviaParser.TryParse("h=L1@(2..8,14..20)", out TriviaBlock block, out _);

        Assert.That(success, Is.True);
        HighlightToken token = block.Tokens[0];
        Assert.That(token.Columns, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(token.Columns![0].Start, Is.EqualTo(SpecBound.Forward(2)));
            Assert.That(token.Columns[0].End, Is.EqualTo(SpecBound.Forward(8)));
            Assert.That(token.Columns[1].Start, Is.EqualTo(SpecBound.Forward(14)));
            Assert.That(token.Columns[1].End, Is.EqualTo(SpecBound.Forward(20)));
        }
    }

    [Test]
    public void BackIndexedColumn_ParsesCorrectly()
    {
        var success = TriviaParser.TryParse("h=L1@2..^2", out TriviaBlock block, out _);

        Assert.That(success, Is.True);
        HighlightToken token = block.Tokens[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(token.Columns![0].Start, Is.EqualTo(SpecBound.Forward(2)));
            Assert.That(token.Columns[0].End, Is.EqualTo(SpecBound.Backward(2)));
        }
    }

    [Test]
    public void BackIndexedLineRange_ParsesCorrectly()
    {
        var success = TriviaParser.TryParse("h=L5-L^1", out TriviaBlock block, out _);

        Assert.That(success, Is.True);
        HighlightToken token = block.Tokens[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(token.Lines[0].Start, Is.EqualTo(SpecBound.Forward(5)));
            Assert.That(token.Lines[0].End, Is.EqualTo(SpecBound.Backward(1)));
        }
    }

    [Test]
    public void FullCombinedExample_ParsesAllPieces()
    {
        var success = TriviaParser.TryParse("h=L1-L3,L5@2..^2", out TriviaBlock block, out _);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(block.Tokens, Has.Count.EqualTo(2));
        }

        HighlightToken first = block.Tokens[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(first.Lines[0].Start, Is.EqualTo(SpecBound.Forward(1)));
            Assert.That(first.Lines[0].End, Is.EqualTo(SpecBound.Forward(3)));
            Assert.That(first.Columns, Is.Null);
        }

        HighlightToken second = block.Tokens[1];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(second.Lines[0].Start, Is.EqualTo(SpecBound.Forward(5)));
            Assert.That(second.Lines[0].IsRange, Is.False);
            Assert.That(second.Columns![0].Start, Is.EqualTo(SpecBound.Forward(2)));
            Assert.That(second.Columns[0].End, Is.EqualTo(SpecBound.Backward(2)));
        }
    }

    [Test]
    public void EmptySpec_FailsWithEmptySpecError()
    {
        var success = TriviaParser.TryParse("h=", out _, out TriviaParseError error);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.False);
            Assert.That(error, Is.EqualTo(TriviaParseError.EmptySpec));
        }
    }

    [Test]
    public void WhitespaceInsideBlock_FailsWithUnexpectedWhitespaceError()
    {
        var success = TriviaParser.TryParse("h= L1-L3", out _, out TriviaParseError error);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.False);
            Assert.That(error, Is.EqualTo(TriviaParseError.UnexpectedWhitespace));
        }
    }

    [Test]
    public void InvertedLineRange_FailsWithInvertedRangeError()
    {
        var success = TriviaParser.TryParse("h=L5-L3", out _, out TriviaParseError error);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.False);
            Assert.That(error, Is.EqualTo(TriviaParseError.InvertedRange));
        }
    }

    [Test]
    public void InvertedColumnRange_FailsWithInvertedRangeError()
    {
        var success = TriviaParser.TryParse("h=L1@8..2", out _, out TriviaParseError error);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.False);
            Assert.That(error, Is.EqualTo(TriviaParseError.InvertedRange));
        }
    }

    [Test]
    public void MissingLPrefix_FailsAsMalformed()
    {
        var success = TriviaParser.TryParse("h=3-5", out _, out TriviaParseError error);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.False);
            Assert.That(error, Is.EqualTo(TriviaParseError.Malformed));
        }
    }

    [Test]
    public void NonNumericBound_FailsWithInvalidNumberError()
    {
        var success = TriviaParser.TryParse("h=Lx", out _, out TriviaParseError error);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.False);
            Assert.That(error, Is.EqualTo(TriviaParseError.InvalidNumber));
        }
    }

    [Test]
    public void SingleLine_Grouped_ParsesCorrectly()
    {
        // parens around a single line-spec (no comma) is valid but degenerate - IsGrouped is set
        // but has no visible effect since there's only one line to apply columns across anyway
        var success = TriviaParser.TryParse("h=(L3)@2..8", out TriviaBlock block, out _);

        Assert.That(success, Is.True);
        HighlightToken token = block.Tokens[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(token.IsGrouped, Is.True);
            Assert.That(token.Lines, Has.Count.EqualTo(1));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(token.Lines[0].Start, Is.EqualTo(SpecBound.Forward(3)));
            Assert.That(token.Lines[0].IsRange, Is.False);
        }
    }

    [Test]
    public void EmptyLineGroup_FailsAsMalformed()
    {
        var success = TriviaParser.TryParse("h=()@2..8", out _, out TriviaParseError error);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.False);
            Assert.That(error, Is.EqualTo(TriviaParseError.Malformed));
        }
    }
}
