namespace ClaimsChat.Services.SealedBox;

// SEALED BOX — not a workshop exercise target.
//
// Result of a grounded chat request: the citations are resolved up front (the
// documents the answer is grounded in), and TextUpdates streams the answer text
// as the model produces it.
public sealed record GroundedChatResult(
    IReadOnlyList<RetrievedDocument> Citations,
    IAsyncEnumerable<string> TextUpdates);
