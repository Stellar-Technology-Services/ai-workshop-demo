using System.Text;

namespace ClaimsChat.Services.SealedBox;

// SEALED BOX — not a workshop exercise target.
//
// Lightweight lexical ranking: tokenize the query and each passage, weight terms
// by inverse document frequency (so LLR boilerplate like "claim"/"coverage" does
// not dominate), and score each passage by the IDF-weighted overlap with the
// query. Returns the top-K passages with score > 0. No embeddings, no external
// search engine.
public static class LexicalRanker
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "and", "for", "are", "was", "were", "this", "that", "with", "from",
        "have", "has", "had", "not", "but", "you", "your", "our", "their", "its",
        "into", "out", "any", "all", "will", "would", "can", "could", "should",
        "what", "which", "who", "when", "where", "how", "about", "there", "been",
        "being", "they", "them", "his", "her", "she", "him",
    };

    public static IReadOnlyList<RetrievedPassage> Rank(
        IReadOnlyList<DocumentPassage> passages, string query, int maxPassages = 3)
    {
        if (passages.Count == 0 || string.IsNullOrWhiteSpace(query) || maxPassages <= 0)
        {
            return Array.Empty<RetrievedPassage>();
        }

        var queryTerms = Tokenize(query).Distinct().ToArray();
        if (queryTerms.Length == 0)
        {
            return Array.Empty<RetrievedPassage>();
        }

        var tokenized = passages.Select(p => Tokenize(p.Text).ToArray()).ToList();
        var passageCount = passages.Count;

        var documentFrequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var tokens in tokenized)
        {
            foreach (var term in tokens.Distinct())
            {
                documentFrequency[term] = documentFrequency.GetValueOrDefault(term) + 1;
            }
        }

        double Idf(string term)
        {
            var freq = documentFrequency.GetValueOrDefault(term);
            return Math.Log((double)passageCount / (1 + freq)) + 1.0;
        }

        var scored = new List<RetrievedPassage>();
        for (var i = 0; i < passageCount; i++)
        {
            var termCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var token in tokenized[i])
            {
                termCounts[token] = termCounts.GetValueOrDefault(token) + 1;
            }

            double score = 0;
            foreach (var term in queryTerms)
            {
                if (termCounts.TryGetValue(term, out var tf) && tf > 0)
                {
                    score += tf * Idf(term);
                }
            }

            if (score <= 0)
            {
                continue;
            }

            // Light length normalization so long sections don't win by size alone.
            score /= Math.Sqrt(tokenized[i].Length + 1);

            var passage = passages[i];
            scored.Add(new RetrievedPassage(
                passage.DocumentId,
                passage.DocumentTitle,
                passage.FileName,
                passage.ClaimNumber,
                passage.SectionTitle,
                passage.Text,
                score));
        }

        return scored
            .OrderByDescending(r => r.Score)
            .Take(maxPassages)
            .ToList();
    }

    private static IEnumerable<string> Tokenize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        var builder = new StringBuilder();
        foreach (var ch in text)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
            else if (builder.Length > 0)
            {
                var token = builder.ToString();
                builder.Clear();
                if (IsAccepted(token))
                {
                    yield return token;
                }
            }
        }

        if (builder.Length > 0)
        {
            var token = builder.ToString();
            if (IsAccepted(token))
            {
                yield return token;
            }
        }
    }

    private static bool IsAccepted(string token) =>
        token.Length >= 3 && !StopWords.Contains(token);
}
