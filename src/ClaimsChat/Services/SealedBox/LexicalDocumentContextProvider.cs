using ClaimsChat.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimsChat.Services.SealedBox;

// SEALED BOX — not a workshop exercise target.
//
// Loads the seeded documents, segments each into passages, and ranks them
// lexically against the query. Thin glue over PassageSegmenter + LexicalRanker.
public sealed class LexicalDocumentContextProvider : IDocumentContextProvider
{
    private readonly IDbContextFactory<ClaimsChatDbContext> _factory;

    public LexicalDocumentContextProvider(IDbContextFactory<ClaimsChatDbContext> factory) =>
        _factory = factory;

    public async Task<IReadOnlyList<RetrievedPassage>> RetrieveAsync(
        string query, int maxPassages = 3, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<RetrievedPassage>();
        }

        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        var documents = await db.Documents
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var passages = new List<DocumentPassage>();
        foreach (var doc in documents)
        {
            foreach (var (sectionTitle, text) in PassageSegmenter.Segment(doc.Body))
            {
                passages.Add(new DocumentPassage(
                    doc.Id, doc.Title, doc.FileName, doc.ClaimNumber, sectionTitle, text));
            }
        }

        return LexicalRanker.Rank(passages, query, maxPassages);
    }
}
