using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace ClaimsChat.Services.SealedBox;

// SEALED BOX — not a workshop exercise target.
//
// Temporary stand-in for a real chat model so the app boots and the Chat page
// works with no Azure AI key configured. It echoes the user's message back.
// Ticket T4 swaps this for a real Azure AI Foundry IChatClient via DI/config;
// the rest of the app talks only to IChatClient and is unaffected by the swap.
public sealed class StubChatClient : IChatClient
{
    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var reply = BuildReply(messages);
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, reply)));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var reply = BuildReply(messages);
        foreach (var token in reply.Split(' '))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new ChatResponseUpdate(ChatRole.Assistant, token + " ");
            await Task.Delay(25, cancellationToken);
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceType?.IsInstanceOfType(this) == true ? this : null;

    public void Dispose()
    {
    }

    private static string BuildReply(IEnumerable<ChatMessage> messages)
    {
        var lastUser = messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? string.Empty;
        return $"[stub reply — no AI model configured] You said: \"{lastUser}\". " +
               "Real grounded answers arrive once Azure AI Foundry is wired up (ticket T4).";
    }
}
