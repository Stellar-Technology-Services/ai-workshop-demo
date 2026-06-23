using System.Text.RegularExpressions;

namespace ClaimsChat.Data;

// Seeds preset claims documents from the Documents folder into SQLite.
// App-layer code (a fair exercise target) — distinct from the sealed-box AI client.
// Idempotent: keyed on FileName, it inserts new files, updates changed ones, and
// removes rows whose source file is gone, so adding/removing a .txt and re-running
// is reflected on next launch.
public static class DocumentSeeder
{
    private static readonly Regex ClaimNumberPattern =
        new(@"^Claim Number:\s*(.+?)\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

    public static void Seed(ClaimsChatDbContext db, string contentRootPath, ILogger? logger = null)
    {
        var directory = Path.Combine(contentRootPath, "Documents");
        if (!Directory.Exists(directory))
        {
            logger?.LogWarning("Documents folder not found at {Directory}; skipping seed.", directory);
            return;
        }

        var existingByName = db.Documents.ToDictionary(d => d.FileName, StringComparer.OrdinalIgnoreCase);
        var presentFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in Directory.GetFiles(directory, "*.txt"))
        {
            var fileName = Path.GetFileName(path);
            presentFileNames.Add(fileName);

            var body = File.ReadAllText(path);
            var title = Path.GetFileNameWithoutExtension(fileName).Replace('_', ' ');
            var claimNumber = ParseClaimNumber(body);

            if (existingByName.TryGetValue(fileName, out var existing))
            {
                // Re-seed updates only the fields parsed from the file. IncludedInRetrieval
                // is deliberately left untouched so a user's eligibility choice (toggled on
                // the Documents page) survives restarts — do not clobber it here.
                if (existing.Body != body || existing.Title != title || existing.ClaimNumber != claimNumber)
                {
                    existing.Body = body;
                    existing.Title = title;
                    existing.ClaimNumber = claimNumber;
                    existing.SeededAtUtc = DateTime.UtcNow;
                }
            }
            else
            {
                db.Documents.Add(new Document
                {
                    FileName = fileName,
                    Title = title,
                    ClaimNumber = claimNumber,
                    Body = body,
                    SeededAtUtc = DateTime.UtcNow,
                });
            }
        }

        foreach (var stale in existingByName.Values.Where(d => !presentFileNames.Contains(d.FileName)))
        {
            db.Documents.Remove(stale);
        }

        var changes = db.SaveChanges();
        logger?.LogInformation("Document seed complete: {FileCount} file(s) on disk, {Changes} row change(s).",
            presentFileNames.Count, changes);
    }

    // Pulls the value from the "Claim Number:" line. A bracketed placeholder
    // (e.g. "[7-digit claim number]") is treated as no claim number.
    private static string? ParseClaimNumber(string body)
    {
        var match = ClaimNumberPattern.Match(body);
        if (!match.Success)
        {
            return null;
        }

        var value = match.Groups[1].Value.Trim();
        if (value.StartsWith('[') && value.EndsWith(']'))
        {
            return null;
        }

        return value.Length == 0 ? null : value;
    }
}
