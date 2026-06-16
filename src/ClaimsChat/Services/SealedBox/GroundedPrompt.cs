using System.Text;
using Microsoft.Extensions.AI;

namespace ClaimsChat.Services.SealedBox;

// SEALED BOX — not a workshop exercise target.
//
// Assembles the chat messages sent to the model: a system instruction that pins
// the answer to the retrieved context (and tells the model to admit when the
// context is empty), the context passages with source labels, then the question.
// Pure and deterministic so it can be unit-tested without calling the model.
public static class GroundedPrompt
{
    private const string SystemInstruction =
        "You are ClaimsChat, an assistant that answers questions about insurance claim documents " +
        "(Large Loss Reports). Answer ONLY using the context passages provided below. " +
        "If the context does not contain the answer, say you could not find it in the claim documents — " +
        "do not guess and do not use outside knowledge. When you answer, cite the claim number and section you used. " +
        "When you cannot answer, briefly guide the user: suggest asking about a specific claim's cause of loss, " +
        "damages, or coverage by describing the event or peril (for example a fire, tornado, or water loss). " +
        "Note that you can answer questions about individual claims, but you cannot total, count, or compare " +
        "figures across all claims.";

    private const string NoContextNotice =
        "No relevant passages were found in the claim documents for this question.";

    public static List<ChatMessage> Build(IReadOnlyList<RetrievedPassage> passages, string question)
    {
        var system = new StringBuilder();
        system.Append(SystemInstruction);
        system.Append("\n\nContext passages:\n");

        if (passages.Count == 0)
        {
            system.Append(NoContextNotice);
        }
        else
        {
            for (var i = 0; i < passages.Count; i++)
            {
                var p = passages[i];
                system.Append($"\n[{i + 1}] Source: {p.DocumentTitle}");
                if (!string.IsNullOrWhiteSpace(p.ClaimNumber))
                {
                    system.Append($" (Claim {p.ClaimNumber})");
                }

                if (!string.IsNullOrWhiteSpace(p.SectionTitle))
                {
                    system.Append($" — Section: {p.SectionTitle}");
                }

                system.Append('\n');
                system.Append(p.Text);
                system.Append('\n');
            }
        }

        return new List<ChatMessage>
        {
            new(ChatRole.System, system.ToString()),
            new(ChatRole.User, question),
        };
    }
}
