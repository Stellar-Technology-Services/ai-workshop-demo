using ClaimsChat.Services.SealedBox;
using Xunit;

namespace ClaimsChat.Tests;

public class LexicalRankerTests
{
    private static IReadOnlyList<DocumentPassage> Corpus() => new List<DocumentPassage>
    {
        new(1, "Claim 1262780", "llr1.txt", "1262780", "DAMAGES",
            "Significant roof and water damage from fire suppression throughout the structure."),
        new(1, "Claim 1262780", "llr1.txt", "1262780", "CONTENTS",
            "Personal property smoke and soot damage; contents inventory pending."),
        new(2, "Claim 1271885", "llr2.txt", "1271885", "INTEREST AND TITLE",
            "First mortgage holder is River Valley Bank with an outstanding balance owed."),
        new(2, "Claim 1271885", "llr2.txt", "1271885", "REINSURANCE FINANCIAL REPORTING",
            "Reinsurance coordinator notified; treaty retention applies to this loss."),
    };

    [Fact]
    public void Rank_QueryMatchingDamages_RanksDamagesPassageFirst()
    {
        var results = LexicalRanker.Rank(Corpus(), "roof water damage", maxPassages: 3);

        Assert.NotEmpty(results);
        Assert.Equal("DAMAGES", results[0].SectionTitle);
        Assert.Equal(1, results[0].DocumentId);
    }

    [Fact]
    public void Rank_QueryMatchingInterest_RanksInterestPassageFirst()
    {
        var results = LexicalRanker.Rank(Corpus(), "mortgage holder", maxPassages: 3);

        Assert.NotEmpty(results);
        Assert.Equal("INTEREST AND TITLE", results[0].SectionTitle);
    }

    [Fact]
    public void Rank_NoOverlap_ReturnsEmpty()
    {
        var results = LexicalRanker.Rank(Corpus(), "xylophone", maxPassages: 3);

        Assert.Empty(results);
    }

    [Fact]
    public void Rank_RespectsMaxPassages()
    {
        var results = LexicalRanker.Rank(Corpus(), "damage", maxPassages: 1);

        Assert.Single(results);
    }
}
