namespace ClaimsChat.Services.SealedBox;

// SEALED BOX — not a workshop exercise target.
//
// The retrieval seam. Given a user question, ranks the seeded documents by
// lexical relevance and returns the most relevant whole documents (full body
// plus the metadata needed for a citation). The lexical implementation is
// deliberately simple; a vector/semantic implementation could be swapped in
// behind this same interface (SPEC §10).
public interface IDocumentContextProvider
{
    Task<IReadOnlyList<RetrievedDocument>> RetrieveDocumentsAsync(
        string query, int maxDocuments = 3, CancellationToken cancellationToken = default);
}
