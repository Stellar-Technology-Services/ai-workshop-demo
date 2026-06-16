namespace ClaimsChat.Services.SealedBox;

// SEALED BOX — not a workshop exercise target.
//
// A single passage of a document, fed into the ranker. Carries source metadata
// so a ranked result can be attributed back to its document for citation.
public sealed record DocumentPassage(
    int DocumentId,
    string DocumentTitle,
    string FileName,
    string? ClaimNumber,
    string? SectionTitle,
    string Text);
