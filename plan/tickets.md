# Build Plan — AI Workshop Demo App

Tickets to build the v1 app described in [`SPEC.md`](../SPEC.md). Ordered by dependency.
Each ticket is meant to be a small, reviewable unit of work that leaves the app runnable.

> Scope reminder: this app is **set dressing** for an AI-assisted-dev workshop. The bar is
> "good enough + legible," not production-grade. The AI client and retrieval are a **sealed box**
> (never an exercise topic); the ordinary app around them is the teachable surface.

## Status legend

`TODO` · `IN PROGRESS` · `DONE` · `BLOCKED`

## Cross-cutting decisions (defaults applied — override if needed)

- **A. Project layout:** single Blazor Server project named **`ClaimsChat`** at **`src/ClaimsChat/`**, with a
  `ClaimsChat.sln` + `global.json` at the repo root. Project folders: `/Data`, `/Services`, `/Components`.
  Sealed-box code lives in a clearly named folder (e.g. `/Services/SealedBox`) so attendees know not to touch it.
- **B. `global.json` roll-forward:** pin the .NET 10 band but allow `rollForward: latestFeature`, so any
  installed `10.0.x` SDK runs the repo (protects locked-down Windows attendees from "SDK not found").
  .NET 10 installs side-by-side with any existing .NET, but is **required** (.NET 8 alone won't satisfy the pin).
- **C. Database creation:** EF Core **migrations**, committed to the repo and auto-applied at startup via
  `Database.Migrate()`. No extra tooling required from attendees at runtime.
- **Render mode:** .NET 10's `dotnet new blazor` scaffolds the new *Blazor Web App*. We configure global
  **Interactive Server** render mode to get classic always-connected Blazor Server (needed for streaming chat over SignalR).

## Dependency map

```
T1 skeleton ──▶ T2 docs+list ──▶ T3 retrieval ──▶ T4 real AI chat ──▶ T5 chat history ──▶ T6 verification
```

---

## T1 — Clone-and-run skeleton  ·  `DONE`

**Goal:** A freshly cloned repo runs with `dotnet run` and serves an empty-but-real Blazor Server app,
with EF Core + SQLite wired up and the database created on first launch. **No AI key required to boot.**
This makes the SPEC §8 "clone → set key → run" guarantee real and provable on Windows.

**Depends on:** nothing.

**Scope**
- Solution + single Blazor Server project (Decision A), global **Interactive Server** render mode.
- `global.json` pinning .NET 10 with lenient roll-forward (Decision B).
- `.gitattributes` forcing **LF**; `.gitignore` (standard .NET + the SQLite `.db` file + `appsettings.Development.json`).
- EF Core + SQLite: a `DbContext`, connection string in `appsettings.json`, initial migration, auto-apply on startup (Decision C).
- A **stub `IChatClient`** (canned/echo reply) registered in DI so the app boots and the chat page "works" without a real key.
- Minimal nav shell with placeholder **Documents** and **Chat** pages (no real functionality yet).
- `README.md`: clone → (optional) set key → `dotnet run`.

**Out of scope:** real documents, seeding content, real retrieval, real AI, citations, chat history.

**Acceptance criteria**
- `git clone` → `dotnet run` with **no secrets set** → app starts; home + both placeholder pages reachable.
- SQLite DB file is created automatically on first run; no manual DB step.
- Runs on macOS and Windows; no shell scripts, no Docker, no LocalDB.
- `.db` file and dev secrets are gitignored (clean `git status` after a run).

---

## T2 — Preset documents + document list view  ·  `DONE`

**Goal:** Curated claims documents (SPEC §4.1–4.2) live in the repo as text/markdown, get seeded into
SQLite on first run, and are browsable in a list view with metadata.

**Depends on:** T1.

**Scope**
- A `Document` entity (id, title, source filename, body text, metadata e.g. created date) + migration.
- A few small curated, IFI-relatable claims documents as `.md`/`.txt` files in the repo (e.g. `Documents/` content folder).
- Idempotent **seeding on startup**: load the preset files into SQLite if not already present.
- **Document list view**: list seeded documents with metadata; click through to a read-only document detail view.

**Out of scope:** upload, PDF parsing, editing/deleting (a delete feature is a candidate *exercise*, not v1), retrieval, AI.

**Acceptance criteria**
- First run seeds the preset documents into SQLite; re-running does not duplicate them.
- Documents page lists all seeded documents with metadata and links to a detail view.
- Adding/removing a preset file and re-running reflects correctly without manual DB steps.

---

## T3 — Lexical retrieval (sealed box)  ·  `DONE`

**Goal:** An `IDocumentContextProvider` that, given a question, selects the most relevant passage(s)
from the seeded documents using lightweight lexical/keyword matching (SPEC §4.3, §5). No embeddings.

**Depends on:** T2.

**Scope**
- `IDocumentContextProvider` interface returning ranked passage(s) + their source document (for citation).
- A lexical implementation (keyword/term overlap scoring) that bounds the number/size of returned passages to keep tokens in check.
- Lives in the sealed-box folder; documented as **off-limits for exercises**.
- Lightweight unit test(s) proving expected passages are selected for a couple of sample questions.

**Out of scope:** embeddings, vector store, semantic ranking (parked — SPEC §10), any UI.

**Acceptance criteria**
- Given a question, the provider returns relevant passage(s) with source attribution.
- Returned context is size-bounded (no dumping whole documents).
- Tests pass for the sample question(s).

**Post-T3 refinements (DONE — from review)**
- Dropped the odd-format receipt doc; corpus is the 10 uniform LLRs (seeder removes the orphan row on next run).
- Passage cap raised to **4000** chars with a boundary-aware splitter (paragraph → line → word); every current
  LLR section (largest 3418) stays whole, and oversized future sections split **without dropping content**
  (replaces the old hard truncation that silently cut the damage narrative).
- All EF Core access converted to **`IDbContextFactory`** (Blazor Server best practice) — see T4 carry-forward.
- Design-time migrations verified working through the factory (`dbcontext info` + `migrations add`/`remove` round-trip).

---

## T4 — Real AI chat with citations  ·  `DONE`

**Shipped:** Azure OpenAI **gpt-5.4** (not Foundry/Haiku — the provider/model changed during the build;
deployment name is a config value, still swappable). `GroundedChatService` + `GroundedPrompt` assemble the
retrieved passages and question, stream the answer, and render **ranked numbered citations**. Assistant
markdown is rendered via **Markdig** (raw HTML disabled). Stub fallback preserved when no key is configured.

**Goal:** Replace the stub with a real `IChatClient` (via `Microsoft.Extensions.AI`). The chat page assembles
retrieved passage(s) + question, calls the model, and **streams** the answer back with a **source
citation** (SPEC §4.3, §6).

**Depends on:** T3.
**Prerequisite (external):** Foundry chat deployment confirmed with quota (SPEC §9) and a key available
out-of-band (SPEC §6). Until then this ticket can be built against the stub and flipped via config/DI.

**Scope**
- Wire `Microsoft.Extensions.AI` `IChatClient` over Azure OpenAI (`Azure.AI.OpenAI`); endpoint/deployment/key
  from config (user-secrets or gitignored `appsettings.Development.json`), model swappable via deployment name.
- Prompt assembly: retrieved passage(s) + question, grounded-answer instruction.
- **Streaming** responses in the Blazor Server chat UI.
- Render the **source citation** alongside the answer.
- Render assistant **markdown** as HTML via **Markdig** (raw HTML disabled for safety).
- Graceful fallback to the stub when no key is configured (keeps clone-and-run intact for non-AI work).

**Out of scope:** chat history persistence (T5), retries/rate-limit UX polish (note throttling per SPEC §7 but keep simple).

**Acceptance criteria**
- With a valid key, asking a question returns a streamed, grounded answer with a correct citation.
- With **no** key, the app still runs and the chat page falls back to the stub (no crash).
- Switching the model name in config changes the deployment used without code changes.

**Carry-forward notes (from T3 + data-access review)**
- **Empty retrieval is a real case:** `RetrieveAsync` returns an **empty list** when no passage overlaps the
  query. Prompt assembly must handle "no relevant documents" deliberately (instruct the model to say it can't
  find it rather than hallucinate) and not conflate it with an error.
- **Citation data is already on hand:** `RetrievedPassage` carries `DocumentId`, `DocumentTitle`, `FileName`,
  `ClaimNumber`, `SectionTitle`, `Score`. Render citations from these (e.g. link to `/documents/{DocumentId}`,
  show `ClaimNumber` + `SectionTitle`); no extra lookup needed.
- **Token budget:** passage cap is 4000 chars; default `maxPassages = 3` → up to ~12K chars (~3K tokens) of
  context per call. Comfortable vs SPEC §7 budget, but keep `maxPassages` modest in prompt assembly.
- **Data access pattern:** any new EF Core access uses `IDbContextFactory` (create/dispose per unit of work);
  do **not** reintroduce a circuit-scoped `DbContext`. `LexicalDocumentContextProvider` is registered scoped
  but depends only on the singleton factory — fine to leave, or make singleton if it gains no scoped deps.

---

## T5 — Chat history persistence  ·  `TODO`

**Goal:** Persist conversations in SQLite so chat history survives restarts (SPEC §4.4).

**Depends on:** T4.

**Scope**
- `Conversation` / `Message` entities (role, content, citation, timestamp) + migration.
- Persist user and assistant messages as a chat proceeds.
- UI: view past conversation(s); resume/continue a conversation (keep minimal/legible).

**Out of scope:** multi-user/auth (SPEC §2), search over history, export (export is a candidate *exercise*).

**Acceptance criteria**
- Messages persist to SQLite and reload after an app restart.
- A new conversation and its messages are correctly associated and ordered.

**Carry-forward note (data-access):** with the `IDbContextFactory` pattern, ensure any entity graph with
navigation properties (e.g. `Conversation` → `Message` → citation) is fully materialized (eager `Include`)
before the per-call context is disposed — no lazy load after `await using` scope ends.

---

## T6 — Cross-platform verification + onboarding polish  ·  `TODO`

**Goal:** Confirm the hard cross-platform requirements (SPEC §8) hold end-to-end and the onboarding story
is airtight for attendees.

**Depends on:** T5.

**Scope**
- Verify on Windows: clone → set key (user-secrets) → `dotnet run` works clean.
- Audit for `Path.Combine` usage, LF endings, no shell scripts, no native deps.
- Finalize `README.md` (setup, key handling, capacity/throttling notes per SPEC §6–§7).
- Confirm `.gitignore` keeps DB + secrets out of source.

**Out of scope:** any new features.

**Acceptance criteria**
- A clean clone runs on both macOS and Windows following only the README.
- No secrets or DB artifacts tracked by git.

---

## Parking lot (NOT v1 — SPEC §10)

Not tickets yet; recorded so they aren't lost.

- Vector embeddings + SQL Server vector search (the real Claims LLR direction).
- Document upload + live ingestion.
- MCP integrations (Context7, Jira).
- Claude Code dual-demo: keep the repo tool-agnostic (`CLAUDE.md` + `.cursorrules` → one source of truth) so it *can* be enabled later.

## Candidate workshop exercises (authored separately — SPEC §11)

Shelf good candidates as they surface during the build. Always at the app layer, never in the sealed box.

- Small feature add (document delete, export a chat transcript).
- Planted mundane bug (sort order, pagination off-by-one, timezone/null-ref).
- Small refactor (clean up a deliberately bloated component/service).
- Scope-creep set piece (vague ticket → plan with AI → bound the creep).
- Add a unit test for one service method.
