using System.Text;
using System.Text.RegularExpressions;

namespace ClaimsChat.Services.SealedBox;

// SEALED BOX — not a workshop exercise target.
//
// Splits a document body into passages. Large Loss Reports use divider lines
// (===== / -----) wrapping an uppercase section title; each title + its body
// becomes a passage. Documents without that structure fall back to blank-line
// paragraphs (and finally the whole document). A section longer than
// MaxPassageChars is split on paragraph -> line -> word boundaries into
// multiple passages (sharing the section title) so token counts stay bounded
// (SPEC §7) without ever dropping content. The cap is sized so every current
// LLR section fits in one passage; splitting is a safety net for outliers.
public static class PassageSegmenter
{
    private const int MaxPassageChars = 4000;

    public static IReadOnlyList<(string? SectionTitle, string Text)> Segment(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return Array.Empty<(string?, string)>();
        }

        var normalized = body.Replace("\r\n", "\n").Replace("\r", "\n");

        // Split into blocks separated by divider lines.
        var blocks = new List<string>();
        var current = new List<string>();
        foreach (var line in normalized.Split('\n'))
        {
            if (IsDivider(line))
            {
                FlushBlock(blocks, current);
                current = new List<string>();
            }
            else
            {
                current.Add(line);
            }
        }
        FlushBlock(blocks, current);

        var passages = new List<(string? SectionTitle, string Text)>();
        string? currentTitle = null;
        var bodyBuilder = new StringBuilder();
        var anyHeader = false;

        void Flush()
        {
            var text = bodyBuilder.ToString().Trim();
            if (text.Length > 0)
            {
                foreach (var chunk in SplitToCap(text))
                {
                    passages.Add((currentTitle, chunk));
                }
            }
            bodyBuilder.Clear();
        }

        foreach (var block in blocks)
        {
            if (IsHeader(block))
            {
                anyHeader = true;
                Flush();
                currentTitle = block;
            }
            else
            {
                if (bodyBuilder.Length > 0)
                {
                    bodyBuilder.Append("\n\n");
                }

                bodyBuilder.Append(block);
            }
        }

        Flush();

        // No LLR section structure detected: fall back to paragraphs.
        return anyHeader ? passages : ParagraphFallback(normalized);
    }

    private static void FlushBlock(List<string> blocks, List<string> current)
    {
        if (current.Count == 0)
        {
            return;
        }

        var text = string.Join("\n", current).Trim();
        if (text.Length > 0)
        {
            blocks.Add(text);
        }
    }

    private static IReadOnlyList<(string?, string)> ParagraphFallback(string body)
    {
        var paragraphs = Regex.Split(body.Trim(), @"\n\s*\n")
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .SelectMany(p => SplitToCap(p).Select(chunk => ((string?)null, chunk)))
            .ToList();

        return paragraphs.Count > 0
            ? paragraphs
            : SplitToCap(body.Trim()).Select(chunk => ((string?)null, chunk)).ToList();
    }

    private static bool IsDivider(string line)
    {
        var trimmed = line.Trim();
        return trimmed.Length >= 5 && (trimmed.All(c => c == '-') || trimmed.All(c => c == '='));
    }

    // A header is a single short line whose letters are all uppercase
    // (e.g. "CLAIM OVERVIEW", "DAMAGES").
    private static bool IsHeader(string block)
    {
        if (block.Contains('\n') || block.Length > 60)
        {
            return false;
        }

        var letters = block.Where(char.IsLetter).ToArray();
        return letters.Length > 0 && letters.All(char.IsUpper);
    }

    // Boundaries to split an over-long section on, coarsest first. Each entry is
    // the regex that separates units and the string used to re-join them.
    private static readonly (string Pattern, string Joiner)[] SplitLevels =
    {
        (@"\n\s*\n", "\n\n"),
        (@"\n", "\n"),
        (@"\s+", " "),
    };

    // Splits text into chunks no longer than MaxPassageChars, preferring the
    // coarsest natural boundary. No non-whitespace content is ever dropped.
    private static IEnumerable<string> SplitToCap(string text)
    {
        text = text.Trim();
        if (text.Length == 0)
        {
            yield break;
        }

        foreach (var chunk in Pack(text, 0))
        {
            yield return chunk;
        }
    }

    private static IEnumerable<string> Pack(string text, int level)
    {
        text = text.Trim();
        if (text.Length <= MaxPassageChars)
        {
            if (text.Length > 0)
            {
                yield return text;
            }

            yield break;
        }

        // Out of natural boundaries: hard-cut as a last resort (pathological input).
        if (level >= SplitLevels.Length)
        {
            for (var i = 0; i < text.Length; i += MaxPassageChars)
            {
                yield return text.Substring(i, Math.Min(MaxPassageChars, text.Length - i));
            }

            yield break;
        }

        var (pattern, joiner) = SplitLevels[level];
        var units = Regex.Split(text, pattern)
            .Select(u => u.Trim())
            .Where(u => u.Length > 0)
            .ToArray();

        // This boundary didn't break the text up; try the next finer one.
        if (units.Length <= 1)
        {
            foreach (var chunk in Pack(text, level + 1))
            {
                yield return chunk;
            }

            yield break;
        }

        var builder = new StringBuilder();
        foreach (var unit in units)
        {
            var pieces = unit.Length <= MaxPassageChars
                ? new[] { unit }
                : Pack(unit, level + 1).ToArray();

            foreach (var piece in pieces)
            {
                if (builder.Length == 0)
                {
                    builder.Append(piece);
                }
                else if (builder.Length + joiner.Length + piece.Length <= MaxPassageChars)
                {
                    builder.Append(joiner).Append(piece);
                }
                else
                {
                    yield return builder.ToString();
                    builder.Clear();
                    builder.Append(piece);
                }
            }
        }

        if (builder.Length > 0)
        {
            yield return builder.ToString();
        }
    }
}
