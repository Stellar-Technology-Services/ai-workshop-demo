using Microsoft.EntityFrameworkCore;

namespace ClaimsChat.Data;

// EF Core context for ClaimsChat. Migrations are committed to the repo and
// applied at startup so a fresh clone creates its own SQLite database.
public class ClaimsChatDbContext : DbContext
{
    public ClaimsChatDbContext(DbContextOptions<ClaimsChatDbContext> options)
        : base(options)
    {
    }

    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Document>()
            .HasIndex(d => d.FileName)
            .IsUnique();
    }
}
