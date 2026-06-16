using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace ClaimsChat.Services.SealedBox;

// SEALED BOX — not a workshop exercise target.
//
// Glue: retrieve passages for the question, assemble the grounded prompt, and
// stream the model's answer. Citations are the retrieved passages, surfaced to
// the UI up front so they can render once the answer finishes.
public sealed class GroundedChatService : IGroundedChatService
{
    private const int MaxPassages = 3;

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
        var passages = await _retrieval.RetrieveAsync(question, MaxPassages, cancellationToken);
        var messages = GroundedPrompt.Build(passages, question);
        return new GroundedChatResult(passages, Stream(messages, cancellationToken));
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
