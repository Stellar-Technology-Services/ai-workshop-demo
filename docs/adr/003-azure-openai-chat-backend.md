# ADR 003 — Azure OpenAI as the chat backend (context-window grounding)

**Status:** Accepted
**Date:** 2026-06-16
**Deciders:** Workshop team

---

## Context

The sealed box (ADR 002) defines `IGroundedChatService` as the seam to generation, but a concrete
chat backend is needed that demonstrates:

- Grounded Q&A over claim documents with citations
- Streaming responses to the Blazor UI
- A clone-and-run experience that boots with **no** API key

## Decision

Implement generation with **Azure OpenAI** through the **`Microsoft.Extensions.AI`** abstraction
(`IChatClient`), wired in `Program.cs`.

**Model / client:** the real client is `AzureOpenAIClient(...).GetChatClient(deployment).AsIChatClient()`,
registered when `AzureOpenAI:Endpoint`, `:Deployment`, and `:Key` are all present in configuration.
The workshop deployment is `gpt-5.4`. The deployment name is a config value, so the model is
swappable without code changes.

**Stub fallback:** when the Azure OpenAI config is absent, `StubChatClient` is registered instead.
The app still boots and the Chat page still works (it echoes), so a fresh clone runs with zero
setup and no secret. This is the "always builds and runs" guarantee in `copilot-instructions.md`.

**Retrieval strategy — context-window grounding:** `LexicalDocumentContextProvider` ranks the
seeded documents and returns the full text of the top matches; `GroundedPrompt` places that text
in the system message and instructs the model to answer only from it and to admit when the answer
is not present. No vector database is required at this demo scale (a handful of short documents).

**Reasoning-model constraint:** the streaming call passes `options: null` — in particular it does
not set `Temperature`, because gpt-5 reasoning models reject non-default values.

## Consequences

- No external NuGet dependency beyond `Azure.AI.OpenAI` + `Microsoft.Extensions.AI(.OpenAI)`.
- Does not scale to thousands of documents — the context-window approach hits token limits. At
  production scale, swap in a vector-search retriever behind the same `IDocumentContextProvider`
  interface; the rest of the app does not change (ADR 002).
- The secret (`AzureOpenAI:Key`) lives only in `appsettings.Development.json` (git-ignored) or user
  secrets — never committed. Permission rules deny reading it (`.claude/settings.json`).
- The human control over *what enters the model's context* is the `IncludedInRetrieval` flag on
  `Document`, toggled in the UI (ADR 002, ADR 004).

## Alternatives rejected

| Option | Why rejected |
|---|---|
| Anthropic Claude via raw HTTP/SSE | The workshop's hosted backend is Azure OpenAI; `Microsoft.Extensions.AI` gives a provider-agnostic seam already |
| Semantic Kernel | Adds significant dependency weight for a workshop demo |
| Vector DB (Qdrant, Pinecone) | Requires infrastructure; overkill at this document count |
| Require a key to boot | Breaks the clone-and-run guarantee; the stub fallback keeps the app runnable with no secret |
