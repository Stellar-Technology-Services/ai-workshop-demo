# ADR 001 — Use SQLite for development and test

**Status:** Accepted
**Date:** 2026-06-16
**Deciders:** Workshop team

---

## Context

The app needs a relational store for the seeded claim documents. For a clone-and-run workshop
demo, the database must:

- Require zero infrastructure setup (no Docker, no server license, no LocalDB)
- Work with EF Core migrations without dialect changes
- Be created and migrated automatically on first launch
- Work identically on macOS, Windows, and Linux

## Decision

Use **SQLite** for local development. The connection string defaults to a single local file
(`Data Source=ClaimsChat.db`) and is overridable via `ConnectionStrings:Default` in
`appsettings.{Environment}.json`. Migrations are committed to the repo and applied on startup
(`db.Database.Migrate()` in `Program.cs`), so a fresh clone creates and seeds its own database
with no manual step.

Tests do **not** use a database. Per `copilot-instructions.md`, the test suite covers pure logic
(ranking, segmentation, prompt building) with xUnit and no mocking — there is no EF provider in
the test project, so there is no per-test database to manage.

## Consequences

- `dotnet run --project src/ClaimsChat` creates `src/ClaimsChat/ClaimsChat.db` on first run; the
  file is git-ignored (`*.db`) and never committed.
- SQLite has no `ALTER COLUMN`; EF Core performs a table rebuild for such changes. Migration
  authors must be aware of this (see the `/create-migration` skill and the `migration-safety` agent).
- Dialect drift is possible against a future SQL Server production target — validate dialect-specific
  SQL before any production deploy.
- CI is fast: no database container required.

## Alternatives considered

| Option | Why rejected |
|---|---|
| SQL Server LocalDB | Requires Windows tooling; incompatible with macOS/Linux dev machines |
| Docker SQL Server in CI | Higher setup cost than a workshop demo warrants; revisit for production readiness |
| PostgreSQL | Not a likely production dialect here; increases drift surface |
| EF Core InMemory for tests | Unnecessary — this repo tests pure logic, not data access |
