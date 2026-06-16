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

    public DateTime SeededAtUtc { get; set; }
}
