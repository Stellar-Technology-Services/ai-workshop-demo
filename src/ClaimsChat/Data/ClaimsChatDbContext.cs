using Microsoft.EntityFrameworkCore;

namespace ClaimsChat.Data;

// EF Core context for ClaimsChat. The schema is intentionally empty in T1 —
// the first entity (Document) arrives in T2. Migrations are committed to the
// repo and applied at startup so a fresh clone creates its own SQLite database.
public class ClaimsChatDbContext : DbContext
{
    public ClaimsChatDbContext(DbContextOptions<ClaimsChatDbContext> options)
        : base(options)
    {
    }
}
