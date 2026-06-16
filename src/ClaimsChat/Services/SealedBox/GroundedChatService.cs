using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace ClaimsChat.Services.SealedBox;

// SEALED BOX — not a workshop exercise target.
//
// Glue: retrieve the most relevant claim documents for the question, assemble
// the grounded prompt from their full text, and stream the model's answer.
// Citations are the retrieved documents, surfaced to the UI up front so they
// can render once the answer finishes.
public sealed class GroundedChatService : IGroundedChatService
{
    private const int MaxDocuments = 3;

    private readonly IDocumentContextProvider _retrieval;
    private readonly IChatClient _chatClient;

    public GroundedChatService(IDocumentContextProvider retrieval, IChatClient chatClient)
    {
        _retrieval = retrieval;
        _chatClient = chatClient;
    }

    public async Task<GroundedChatResult> AskAsync(
        string question, CancellationToken cancellationToken = default)
    {
        var documents = await _retrieval.RetrieveDocumentsAsync(question, MaxDocuments, cancellationToken);
        var messages = GroundedPrompt.Build(documents, question);
        return new GroundedChatResult(documents, Stream(messages, cancellationToken));
    }

    private async IAsyncEnumerable<string> Stream(
        List<ChatMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // No ChatOptions: in particular do not set Temperature — gpt-5 reasoning
        // models reject non-default values.
        await foreach (var update in _chatClient.GetStreamingResponseAsync(messages, options: null, cancellationToken))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                yield return update.Text;
            }
        }
    }
}
