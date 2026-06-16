namespace ClaimsChat.Services.SealedBox;

// SEALED BOX — not a workshop exercise target.
//
// A whole document surfaced by retrieval: the lexical ranker finds the most
// relevant passage(s), then the best-matching documents are expanded to their
// full body so the model gets every section of the claim (overview, coverage,
// damages, financials) in one place. TopSectionTitle/Score come from the
// document's highest-scoring passage and drive the citation.
public sealed record RetrievedDocument(
    int DocumentId,
    string DocumentTitle,
    string FileName,
    string? ClaimNumber,
    string? TopSectionTitle,
    string Body,
    double Score);
