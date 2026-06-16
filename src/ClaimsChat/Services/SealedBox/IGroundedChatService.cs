namespace ClaimsChat.Services.SealedBox;

// SEALED BOX — not a workshop exercise target.
//
// Orchestrates a grounded answer: retrieve relevant passages, assemble the
// prompt, and stream the model's answer. The UI consumes only this seam.
public interface IGroundedChatService
{
    Task<GroundedChatResult> AskAsync(string question, CancellationToken cancellationToken = default);
}
