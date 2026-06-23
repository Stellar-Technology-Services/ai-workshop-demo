---
name: create-migration
description: Scaffold a safe EF Core (SQLite) migration for a schema change in ClaimsChat, with correct nullable/default handling, a reversible Down(), and an explicit strategy for existing rows. Use whenever a Document field or any entity/schema change is needed.
---

# create-migration

Produce a safe EF Core migration for every schema change, with nullable handling, a reversible
`Down()`, and an explicit strategy for existing rows. This repo targets **.NET 10 / EF Core 10 /
SQLite**; migrations are committed and applied automatically on startup (`db.Database.Migrate()`
in `Program.cs`).

## How to invoke

Describe the schema change, naming the entity and the field:

```
/create-migration

Add a bool IncludedInRetrieval (default true) to Document
```

## What this skill does

1. Edits the entity in `src/ClaimsChat/Data/` (e.g. `Document.cs`) to add the property.
2. Sets the default in `ClaimsChatDbContext.OnModelCreating` so EF and the migration agree.
3. Runs `dotnet ef migrations add <Name> --project src/ClaimsChat`.
4. Verifies the generated `Up()`/`Down()` — a SQL-level `defaultValue` so existing rows get a value
   immediately, and a `Down()` that fully reverses `Up()`.
5. Flags SQLite rebuild surprises (SQLite has no `ALTER COLUMN`).

## Worked example — the `IncludedInRetrieval` flag

**Entity** (`src/ClaimsChat/Data/Document.cs`):

```csharp
// New documents default to eligible; the seeder must not clobber a user-set value.
public bool IncludedInRetrieval { get; set; } = true;
```

**Context** (`src/ClaimsChat/Data/ClaimsChatDbContext.cs`, in `OnModelCreating`):

```csharp
// NOT NULL with a SQL-level default so existing rows become eligible when the column is added.
modelBuilder.Entity<Document>()
    .Property(d => d.IncludedInRetrieval)
    .HasDefaultValue(true);
```

**Generate it:**

```bash
dotnet ef migrations add AddIncludedInRetrievalToDocuments --project src/ClaimsChat
```

**Verify the generated migration** (`src/ClaimsChat/Data/Migrations/<timestamp>_AddIncludedInRetrievalToDocuments.cs`):

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // SQLite stores bool as INTEGER. defaultValue: true → existing rows get 1 (eligible).
    migrationBuilder.AddColumn<bool>(
        name: "IncludedInRetrieval",
        table: "Documents",
        type: "INTEGER",
        nullable: false,
        defaultValue: true);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "IncludedInRetrieval",
        table: "Documents");
}
```

## Safety checklist (verify before relying on the migration)

- [ ] `NOT NULL` columns have a `defaultValue` — EF does not infer it from the C# initializer.
- [ ] The migration's `defaultValue` matches the `HasDefaultValue()` in `OnModelCreating`.
- [ ] `Down()` is present and reverses every change in `Up()`.
- [ ] Existing rows are handled — via `defaultValue`, a seeded `Sql()` update, or an explicit
      decision that null is acceptable (nullable columns only).
- [ ] No unintended SQLite table rebuild — SQLite has no `ALTER COLUMN`; type/rename changes are
      drop+add by default. Confirm whether data must be preserved.
- [ ] The migration applies cleanly on a fresh local DB before committing.

## Rules

- Never run `dotnet ef database update` (or `drop`) against a shared/production database without a
  reviewed migration file. `dotnet ef database drop` is denied by the agent permission rules.
- **Read the generated `.cs` file before relying on it.** The migration is the evidence; the chat
  summary is not. For a second opinion, hand the migration to the `migration-safety` agent.
