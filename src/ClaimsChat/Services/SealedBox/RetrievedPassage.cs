namespace ClaimsChat.Services.SealedBox;

// SEALED BOX — not a workshop exercise target.
//
// A passage selected by retrieval, with its relevance score and the source
// metadata needed to render an "answer + citation".
public sealed record RetrievedPassage(
    int DocumentId,
    string DocumentTitle,
    string FileName,
    string? ClaimNumber,
    string? SectionTitle,
    string Text,
    double Score);
