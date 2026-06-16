using System.Text.RegularExpressions;
using ClaimsChat;
using ClaimsChat.Services.SealedBox;
using Xunit;

namespace ClaimsChat.Tests;

// Proves every starter chip (SuggestedQuestions.All) reliably retrieves its
// intended claim through the real lexical pipeline (PassageSegmenter + LexicalRanker)
// over the actual document corpus. This validates the deterministic retrieval seam
// — the thing the model's answer depends on — not the model's generated text.
public class SuggestedQuestionTests
{
    // Mirrors GroundedChatService.MaxPassages: the chip's target claim must be in
    // the passages the model will actually see.
    private const int MaxPassages = 3;

    // Each shipped chip maps to the claim it must surface. Keyed by exact chip text,
    // so adding a chip without an expectation here fails AllChips_HaveAnExpectation.
    private static readonly Dictionary<string, string> ExpectedClaimByQuestion = new()
    {
        ["What caused the damage in the tornado claim?"] = "1286517",   // Garrett
        ["Summarize the lightning strike claim."] = "1284326",          // Patel
        ["What damage did the vehicle cause to the garage?"] = "1268493", // Dalton
        ["Tell me about the vandalism and graffiti claim."] = "1262780",// Swanson
    };

    [Fact]
    public void AllChips_HaveAnExpectation()
    {
        foreach (var question in SuggestedQuestions.All)
        {
            Assert.True(
                ExpectedClaimByQuestion.ContainsKey(question),
                $"Suggested question '{question}' has no expected-claim entry in SuggestedQuestionTests.");
        }
    }

    [Fact]
    public void EveryChip_RetrievesItsTargetClaim()
    {
        var passages = LoadCorpusPassages();

        foreach (var question in SuggestedQuestions.All)
        {
            var expectedClaim = ExpectedClaimByQuestion[question];
            var ranked = LexicalRanker.Rank(passages, question, MaxPassages);

            Assert.True(ranked.Count > 0, $"No passages retrieved for '{question}'.");
            Assert.True(ranked[0].Score > 0, $"Top passage for '{question}' scored 0.");

            var topClaims = ranked.Select(r => r.ClaimNumber).ToList();
            Assert.True(
                topClaims.Contains(expectedClaim),
                $"'{question}' expected claim {expectedClaim} in top {MaxPassages}, got: " +
                string.Join(", ", topClaims));

            Assert.Equal(expectedClaim, ranked[0].ClaimNumber);
        }
    }

    private static List<DocumentPassage> LoadCorpusPassages()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "Documents");
        Assert.True(Directory.Exists(directory), $"Test corpus folder not found at {directory}.");

        var files = Directory.GetFiles(directory, "*.txt");
        Assert.True(files.Length > 0, "No claim documents found in the test corpus.");

        var passages = new List<DocumentPassage>();
        var docId = 0;
        foreach (var path in files)
        {
            docId++;
            var fileName = Path.GetFileName(path);
            var title = Path.GetFileNameWithoutExtension(fileName).Replace('_', ' ');
            var claimNumber = ParseClaimNumber(fileName);
            var body = File.ReadAllText(path);

            foreach (var (sectionTitle, text) in PassageSegmenter.Segment(body))
            {
                passages.Add(new DocumentPassage(docId, title, fileName, claimNumber, sectionTitle, text));
            }
        }

        return passages;
    }

    private static string? ParseClaimNumber(string fileName)
    {
        var match = Regex.Match(fileName, @"LLR_(\d+)_");
        return match.Success ? match.Groups[1].Value : null;
    }
}
