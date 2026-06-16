using ClaimsChat.Services.SealedBox;
using Microsoft.Extensions.AI;
using Xunit;

namespace ClaimsChat.Tests;

public class GroundedPromptTests
{
    private static RetrievedPassage Passage(string title, string claim, string section, string text) =>
        new(DocumentId: 1, DocumentTitle: title, FileName: $"{title}.txt",
            ClaimNumber: claim, SectionTitle: section, Text: text, Score: 1.0);

    [Fact]
    public void Build_WithPassages_IncludesSourceLabelsAndText()
    {
        var passages = new[]
        {
            Passage("LLR 1262780 Swanson", "1262780", "DAMAGES", "Significant roof and water damage."),
        };

        var messages = GroundedPrompt.Build(passages, "What was the damage?");

        var system = messages.Single(m => m.Role == ChatRole.System).Text;
        Assert.Contains("Significant roof and water damage.", system);
        Assert.Contains("1262780", system);
        Assert.Contains("DAMAGES", system);
    }

    [Fact]
    public void Build_PutsQuestionAsUserMessage()
    {
        var messages = GroundedPrompt.Build(
            new[] { Passage("Doc", "1", "S", "body") }, "What was the damage?");

        var user = messages.Single(m => m.Role == ChatRole.User);
        Assert.Equal("What was the damage?", user.Text);
    }

    [Fact]
    public void Build_NoPassages_TellsModelItCouldNotFind()
    {
        var messages = GroundedPrompt.Build(Array.Empty<RetrievedPassage>(), "Anything?");

        var system = messages.Single(m => m.Role == ChatRole.System).Text;
        Assert.Contains("could not find it in the claim documents", system);
        Assert.Contains("No relevant passages were found", system);
    }
}
