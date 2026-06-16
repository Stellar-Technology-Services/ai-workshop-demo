# SPEC — AI Workshop Demo App

## 1. Purpose & context

This is a **teaching vehicle** for an AI-assisted development workshop for ~21 developers at IFI.

- The workshop teaches **AI-assisted development workflows** (planning a ticket, guiding the agent, reviewing, avoiding scope creep) — **not** how AI/RAG works internally.
- The app is deliberately **set dressing**: a believable, IFI-relatable "chat with claims documents" experience that the workshop *operates on*. Its job is to be legible and "good enough," with room for planted exercises — not to be a fully-fledged product.
- It doubles as a **directional preview** of the Claims LLR/retrieval idea, without being a real RAG implementation.

**Environments:** We develop on **macOS in Cursor**. Attendees **clone and run on Windows** (some in VS Code). **Cross-platform, clone-and-run seamlessness is a hard requirement.**

## 2. Non-goals (explicit — to prevent scope creep)

These are intentionally **out of scope for v1**:

- No embeddings, vector store, or RAG internals.
- No document upload (preset documents only).
- No PDF parsing (documents are stored as text/markdown).
- No authentication or multi-user support.
- No external database infrastructure (no SQL Server, LocalDB, or Docker).
- No MCP integrations (Jira / Context7).
- Not a production-grade app. "Good enoughfor training + legible" is the bar.

## 3. Stack

| Concern | Choice | Rationale |
|---|---|---|
| Language / runtime | **C# / .NET 10 LTS**, pinned via `global.json` | Matches the audience (C#/.NET = 18/19); current LTS (supported to Nov 2028), installs side-by-side with any existing .NET on locked-down corporate Windows machines |
| UI | **Blazor Server** | Keeps streaming chat in pure C# (audience is JS-light); single language end-to-end |
| Persistence | **EF Core + SQLite** | Real ORM the team knows, zero infra, cross-platform clone-and-run; SQL Server is a connection-string swap if ever wanted |
| Chat AI | **Azure OpenAI**, model **gpt-5.4** (deployment name is a config value; model swappable) | Enterprise-relatable; model is a config value |
| AI client | **`Microsoft.Extensions.AI` (`IChatClient`)** over the Azure OpenAI SDK (`Azure.AI.OpenAI`) | Idiomatic .NET abstraction; model/provider stays a config value |
| Retrieval | **Lightweight lexical / keyword matching** behind `IDocumentContextProvider` (ranks passages, then grounds answers in the full top-ranked documents) | No embeddings; gives grounded "answer + citation"; vector retrieval is a clean future swap |
| Answer rendering | **Markdig** (raw HTML disabled) | Renders the model's markdown safely in the Blazor UI |

## 4. Core functionality (v1)

1. **Preset documents** — a few small, curated, claims-related documents stored as plain **text/markdown** in the repo, seeded into SQLite on first run.
2. **Document list view** — see the preset documents and their metadata.
3. **Chat view** — ask a question (or click a **suggested-question chip**); lexical retrieval ranks passages and expands the best matches to their **full parent documents**; those documents + the question are sent to the chat model; the answer streams back (rendered as markdown) with **ranked source citations**. The grounding prompt keeps the model on the supplied documents and has it redirect questions it can't answer (it handles individual claims, not cross-claim aggregates).
4. **Chat history** — persisted in SQLite. *(Planned — T5; not yet built.)*

## 5. Architecture & seams

Pipeline (intentionally legible):

```
preset docs (seeded → SQLite)
        ↓
IDocumentContextProvider   ← sealed box: lexical retrieval → full top-ranked documents (never an exercise topic)
        ↓
chat service (Microsoft.Extensions.AI / IChatClient)   ← sealed box: grounded prompt + AI call
        ↓
Blazor Server UI (streaming chat, markdown, citations)   ← teachable surface
```

- **Sealed box:** retrieval and the AI client sit behind interfaces and are **never the subject of a workshop exercise**.
- **Teachable surface:** the ordinary app around the box — UI, data layer, app features — where planted exercises live.

## 6. AI configuration & secrets

- A **shared Azure OpenAI key**, distributed **out-of-band** at workshop time.
- Stored in a **gitignored** `appsettings.Development.json` (copy the committed
  `appsettings.Development.example.json` template) or **.NET user-secrets**. **Never committed.**
- Config values (`AzureOpenAI` section): `Endpoint` (base host), `Deployment` name, `Key`. Model is swappable via the deployment name.

## 7. Capacity planning (~21 devs)

- The binding constraint is **burst concurrency / tokens-per-minute**, not cost.
- Provision roughly **~150K TPM and ~250 RPM** on the chat deployment to absorb the "everyone follows along at once" burst pattern.
- Cost is trivial (single-digit dollars for the event). Lexical retrieval keeps per-message token counts bounded.
- **Keep a backup key** available in case the deployment throttles mid-session.

## 8. Cross-platform seamlessness (hard requirements)

- `global.json` pins the **.NET 10 SDK** (prevents "works on my machine").
- `.gitattributes` forces **LF** line endings.
- Use `Path.Combine` — no hardcoded path separators.
- **No shell scripts** for setup; use `dotnet` commands only.
- **No native modules, no Docker, no LocalDB.**
- Onboarding target: **clone → set key (user-secrets) → `dotnet run`.**

## 9. Pre-build verification (confirm before/while building)

1. Can every attendee install the **.NET 10 SDK** on their (likely locked-down) Windows machine? (Installs side-by-side with any existing .NET; `global.json` pins .NET 10, so .NET 10 specifically is required.)
2. Confirm the **Azure OpenAI chat deployment** (gpt-5.4) exists with adequate quota (see §7).
3. Confirm the shared key distribution plan (out-of-band, never committed).

## 10. Deferred / parking lot (NOT v1)

- Vector embeddings + SQL Server vector search (the real Claims LLR direction).
- Document upload + live ingestion.
- MCP integrations (Context7, Jira).
- **Claude Code dual-demo:** build the repo **tool-agnostic** (a `CLAUDE.md` + `.cursorrules` pointing at one source of truth) so it *can* be enabled later — but it is **not** a v1 deliverable.

## 11. Workshop exercises (authored separately — not built into v1)

Candidate planted tasks live at the **app layer**, never inside the sealed box. To be curated separately as demo material; shelf good candidates as they surface during the build:

- Small **feature add** (e.g., document list with delete, export a chat transcript).
- A planted **mundane bug** (sort order, pagination off-by-one, timezone/null-ref).
- A small **refactor** (clean up a deliberately bloated component/service).
- The **scope-creep set piece** (vague ticket → plan with AI → bound the creep).
- Add a **unit test** for one service method.
