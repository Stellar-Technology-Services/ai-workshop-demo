namespace ClaimsChat.Services.SealedBox;

// SEALED BOX — not a workshop exercise target.
//
// The retrieval seam. Given a user question, returns the most relevant passage(s)
// from the seeded documents, with enough source metadata to build a citation.
// The lexical implementation is deliberately simple; a vector/semantic
// implementation could be swapped in behind this same interface (SPEC §10).
public interface IDocumentContextProvider
{
    Task<IReadOnlyList<RetrievedPassage>> RetrieveAsync(
        string query, int maxPassages = 3, CancellationToken cancellationToken = default);
}
