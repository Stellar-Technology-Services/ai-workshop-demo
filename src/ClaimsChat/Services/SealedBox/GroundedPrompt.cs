using System.Text;
using Microsoft.Extensions.AI;

namespace ClaimsChat.Services.SealedBox;

// SEALED BOX — not a workshop exercise target.
//
// Assembles the chat messages sent to the model: a system instruction that pins
// the answer to the retrieved context (and tells the model to admit when the
// context is empty), the full text of the most relevant claim documents with
// source labels, then the question. Pure and deterministic so it can be
// unit-tested without calling the model.
public static class GroundedPrompt
{
    private const string SystemInstruction =
        "You are ClaimsChat, an assistant that answers questions about insurance claim documents " +
        "(Large Loss Reports). Answer ONLY using the claim documents provided below. " +
        "If the context does not contain the answer, say you could not find it in the claim documents — " +
        "do not guess and do not use outside knowledge. When you answer, cite the claim number and section you used. " +
        "When you cannot answer, briefly guide the user: suggest asking about a specific claim's cause of loss, " +
        "damages, or coverage by describing the event or peril (for example a fire, tornado, or water loss). " +
        "Note that you can answer questions about individual claims, but you cannot total, count, or compare " +
        "figures across all claims.";

    private const string NoContextNotice =
        "No relevant claim documents were found for this question.";

    public static List<ChatMessage> Build(IReadOnlyList<RetrievedDocument> documents, string question)
    {
        var system = new StringBuilder();
        system.Append(SystemInstruction);
        system.Append("\n\nClaim documents:\n");

        if (documents.Count == 0)
        {
            system.Append(NoContextNotice);
        }
        else
        {
            for (var i = 0; i < documents.Count; i++)
            {
                var doc = documents[i];
                system.Append($"\n[{i + 1}] Source: {doc.DocumentTitle}");
                if (!string.IsNullOrWhiteSpace(doc.ClaimNumber))
                {
                    system.Append($" (Claim {doc.ClaimNumber})");
                }

                system.Append('\n');
                system.Append(doc.Body);
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
