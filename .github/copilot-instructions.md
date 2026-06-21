# ClaimsChat: Copilot instructions

ClaimsChat is a small "chat with claims documents" web app. It is the teaching vehicle for an
AI-assisted development workshop, so the bar is "legible and good enough," not production-grade.
See [`SPEC.md`](../SPEC.md) for scope and [`plan/tickets.md`](../plan/tickets.md) for the build plan.

## Tech stack

- **C# / .NET 10** (pinned by [`global.json`](../global.json)). Nullable and implicit usings are enabled.
- **ASP.NET Core Blazor Server** (Interactive Server render mode) for the UI.
- **EF Core 10 + SQLite** for persistence; migrations are applied automatically on startup.
- **Microsoft.Extensions.AI** over **Azure OpenAI** for chat; **Markdig** for rendering answers.
- **xUnit** for tests.

## Project structure

- `src/ClaimsChat/` is the app: `Components/` (Blazor pages + layout), `Data/` (EF Core context,
  entities, migrations, seeder), `Services/` (app services), `Documents/` (seeded claim `.txt` files).
- `src/ClaimsChat/Services/SealedBox/` holds retrieval and the AI client. **Off-limits**, see below.
- `tests/ClaimsChat.Tests/` holds the xUnit tests.
- `workshop/` and `plan/` are docs, not application code.

## Build, test, run

- Build: `dotnet build`
- Test: `dotnet test`
- Run: `dotnet run --project src/ClaimsChat`
- Add a migration: `dotnet ef migrations add <Name> --project src/ClaimsChat`

The app boots without an AI key (the chat falls back to a stub), so it always builds and runs.

## Conventions

- Use **file-scoped namespaces** and keep nullable reference types satisfied (no `!` to silence warnings).
- Data access goes through **`IDbContextFactory<ClaimsChatDbContext>`**, creating and disposing a
  context per unit of work. Do **not** inject a circuit-scoped `DbContext` (it breaks under Blazor
  Server concurrency). Eagerly load (`Include`) any navigation you need before the context is disposed.
- Prefer `async`/`await` end to end; pass `CancellationToken` through.
- Cross-platform: use `Path.Combine`, keep **LF** line endings, and add **no shell scripts** for setup.
- Match the style of the file you are editing; look at neighboring code before introducing new patterns.

## The sealed box (architectural boundary)

`Services/SealedBox/` (lexical retrieval + the AI client) sits behind interfaces and is **not a place
to make changes**. You may **scope what data reaches it** (for example, filtering which documents are
eligible), but do not rewrite the ranking algorithm or the AI call. Treat `IDocumentContextProvider`
and `IGroundedChatService` as fixed contracts.

## Tests

- Put tests in `tests/ClaimsChat.Tests/`, one behavior per test, arrange/act/assert, no mocking framework.
- Favor testing pure logic (segmentation, ranking, prompt building) over wiring.
- Run `dotnet test` and make sure it passes before declaring work done.
