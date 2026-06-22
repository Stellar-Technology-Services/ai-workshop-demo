namespace ClaimsChat.Data;

// A preset claims document (e.g. a Large Loss Report) seeded from a .txt file
// in the Documents folder. FileName is the stable key used by the seeder.
public class Document
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? ClaimNumber { get; set; }

    public string Body { get; set; } = string.Empty;

    // The human checkpoint for what enters retrieval: when false, the sealed box
    // never sees this document, so the model cannot ground answers in it. Users
    // toggle it on the Documents page. New documents default to eligible (true).
    // The seeder must not clobber a user-set value on re-seed (see DocumentSeeder).
    public bool IncludedInRetrieval { get; set; } = true;

    public DateTime SeededAtUtc { get; set; }
}
