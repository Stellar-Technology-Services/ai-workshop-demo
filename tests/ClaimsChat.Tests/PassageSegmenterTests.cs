using ClaimsChat.Services.SealedBox;
using Xunit;

namespace ClaimsChat.Tests;

public class PassageSegmenterTests
{
    private const string SampleLlr = """
        ================================================================================
        LARGE LOSS REPORT (LLR) — PROPERTY
        ================================================================================

        Report Type: Closing Report
        Reinsurance Notified: No

        --------------------------------------------------------------------------------
        CLAIM OVERVIEW
        --------------------------------------------------------------------------------

        Name of Adjuster: Thomas Beckett
        Claim Number: 1262780

        --------------------------------------------------------------------------------
        LOSS INFORMATION
        --------------------------------------------------------------------------------

        Date of Loss: 03/14/2026
        Cause of Loss: Kitchen fire spread to the roof.

        --------------------------------------------------------------------------------
        DAMAGES
        --------------------------------------------------------------------------------

        Structure: Significant roof and water damage from fire suppression.
        """;

    [Fact]
    public void Segment_LlrDocument_SplitsIntoSectionsWithTitles()
    {
        var passages = PassageSegmenter.Segment(SampleLlr);

        var titles = passages.Select(p => p.SectionTitle).ToArray();
        Assert.Equal(
            new[] { "LARGE LOSS REPORT (LLR) — PROPERTY", "CLAIM OVERVIEW", "LOSS INFORMATION", "DAMAGES" },
            titles);
    }

    [Fact]
    public void Segment_LlrDocument_CapturesSectionBody()
    {
        var passages = PassageSegmenter.Segment(SampleLlr);

        var overview = passages.Single(p => p.SectionTitle == "CLAIM OVERVIEW");
        Assert.Contains("Thomas Beckett", overview.Text);

        var damages = passages.Single(p => p.SectionTitle == "DAMAGES");
        Assert.Contains("roof and water damage", damages.Text);
    }

    [Fact]
    public void Segment_PlainTextWithoutStructure_ReturnsSingleFallbackPassage()
    {
        const string note = "Just a simple note with no dividers and no blank lines here.";

        var passages = PassageSegmenter.Segment(note);

        var passage = Assert.Single(passages);
        Assert.Null(passage.SectionTitle);
        Assert.Contains("simple note", passage.Text);
    }

    [Fact]
    public void Segment_EmptyInput_ReturnsNoPassages()
    {
        Assert.Empty(PassageSegmenter.Segment("   "));
    }

    [Fact]
    public void Segment_SectionLongerThanCap_SplitsAcrossPassagesWithoutDroppingContent()
    {
        var longBody = string.Join(
            "\n\n",
            Enumerable.Range(1, 80).Select(i =>
                $"Paragraph {i}: structural damage assessment detail describing the affected area in depth."));

        var doc = $"""
            ================================================================================
            LARGE LOSS REPORT (LLR) — PROPERTY
            ================================================================================

            --------------------------------------------------------------------------------
            DAMAGES
            --------------------------------------------------------------------------------

            {longBody}
            """;

        var damageChunks = PassageSegmenter.Segment(doc)
            .Where(p => p.SectionTitle == "DAMAGES")
            .ToArray();

        Assert.True(damageChunks.Length > 1, "Oversized section should split into multiple passages.");
        Assert.All(damageChunks, p => Assert.True(p.Text.Length <= 4000, $"Chunk exceeded cap: {p.Text.Length}"));

        static string Strip(string s) => new(s.Where(c => !char.IsWhiteSpace(c)).ToArray());
        Assert.Equal(Strip(longBody), Strip(string.Concat(damageChunks.Select(p => p.Text))));
    }
}
