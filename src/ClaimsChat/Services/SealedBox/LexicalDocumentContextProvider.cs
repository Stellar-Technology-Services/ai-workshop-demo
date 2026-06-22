using ClaimsChat.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimsChat.Services.SealedBox;

// SEALED BOX — not a workshop exercise target.
//
// Loads the seeded documents, segments each into passages, and ranks them
// lexically against the query. The top-scoring passages are then expanded to
// their full parent documents (deduped, highest-scoring passage per document
// wins) so the model receives every section of the most relevant claims. Thin
// glue over PassageSegmenter + LexicalRanker.
public sealed class LexicalDocumentContextProvider : IDocumentContextProvider
{
    private readonly IDbContextFactory<ClaimsChatDbContext> _factory;

    public LexicalDocumentContextProvider(IDbContextFactory<ClaimsChatDbContext> factory) =>
        _factory = factory;

    public async Task<IReadOnlyList<RetrievedDocument>> RetrieveDocumentsAsync(
        string query, int maxDocuments = 3, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || maxDocuments <= 0)
        {
            return Array.Empty<RetrievedDocument>();
        }

        await using var db = await _factory.CreateDbContextAsync(cancellationToken);

        // DATA-SCOPE SEAM (app concern, not the ranking algorithm): only documents the
        // user has marked eligible reach the box. This filters *what* is ranked; the
        // ranking, segmentation, prompt, and AI call below are unchanged. See the
        // IncludedInRetrieval flag on Document and copilot-instructions.md.
        var documents = await db.Documents
            .AsNoTracking()
            .Where(d => d.IncludedInRetrieval)
            .ToListAsync(cancellationToken);

        var bodyById = documents.ToDictionary(d => d.Id, d => d.Body);

        var passages = new List<DocumentPassage>();
        foreach (var doc in documents)
        {
            foreach (var (sectionTitle, text) in PassageSegmenter.Segment(doc.Body))
            {
                passages.Add(new DocumentPassage(
                    doc.Id, doc.Title, doc.FileName, doc.ClaimNumber, sectionTitle, text));
            }
        }

        // Rank every passage, then collapse to the best passage per document.
        // Passages arrive ordered by descending score, so the first time a
        // document appears is its top-scoring section.
        var ranked = LexicalRanker.Rank(passages, query, passages.Count);

        var seen = new HashSet<int>();
        var results = new List<RetrievedDocument>();
        foreach (var passage in ranked)
        {
            if (!seen.Add(passage.DocumentId))
            {
                continue;
            }

            results.Add(new RetrievedDocument(
                passage.DocumentId,
                passage.DocumentTitle,
                passage.FileName,
                passage.ClaimNumber,
                passage.SectionTitle,
                bodyById.GetValueOrDefault(passage.DocumentId, passage.Text),
                passage.Score));

            if (results.Count >= maxDocuments)
            {
                break;
            }
        }

        return results;
    }
}
