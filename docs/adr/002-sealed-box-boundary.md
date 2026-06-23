# ADR 002 — Retrieval + AI behind a sealed-box boundary

**Status:** Accepted
**Date:** 2026-06-16
**Deciders:** Workshop team

---

## Context

ClaimsChat answers questions about claim documents with a retrieval step (rank the seeded
documents by relevance) followed by a generation step (the model answers from the retrieved
text, with citations). This retrieval + AI machinery is the part of the app a workshop attendee
is *most* likely to break and *least* likely to need to change. It must be insulated from
ordinary feature work.

The risk: without a clear boundary, feature developers couple UI/service logic to retrieval
internals (the ranking algorithm, the prompt assembly, the model call), making both harder to
test and evolve — and turning every feature exercise into an accidental rewrite of the hard part.

## Decision

Treat everything in `src/ClaimsChat/Services/SealedBox/` as a **sealed box** with two interfaces
as its only seams:

- `IDocumentContextProvider` — given a query, returns the most relevant whole documents.
  Implemented by `LexicalDocumentContextProvider` (passage segmentation + TF-IDF-style ranking).
- `IGroundedChatService` — orchestrates retrieve → assemble prompt → stream the answer with
  citations. Implemented by `GroundedChatService`.

Rules:

- The ranking algorithm (`LexicalRanker`), segmentation (`PassageSegmenter`), prompt assembly
  (`GroundedPrompt`), and the AI call are **off-limits during feature work.**
- The UI depends only on `IGroundedChatService`; it receives citations and a streamed answer.
- Feature work **may scope what data reaches the box** — for example, filtering which documents
  are eligible. The `IncludedInRetrieval` flag on `Document` is exactly this: it changes *what*
  the box ranks over, not *how* it ranks. The eligibility filter lives at the box's document-load
  query and is the one sanctioned touch point for the "scope the inputs" exercise.

## Consequences

- Attendees can work on data, service, and UI layers without rewriting retrieval.
- A vector/semantic retriever could replace the lexical one behind the same
  `IDocumentContextProvider` interface with no change to the rest of the app (see SPEC §10).
- **Rule for reviewers:** any diff that rewrites the ranking/segmentation/prompt/AI call inside
  the sealed box should be flagged `[HIGH]` in `/code-review`. Scoping *inputs* (eligibility) is
  fine; rewriting the *mechanism* is not.

## The "legacy seam" parallel

This boundary plays the same role as a legacy module in an existing system: it encapsulates
behavior that is correct but opaque. The right posture is `/explain-legacy` — understand what it
does and what breaks if removed — not refactor it during unrelated feature work.

## Alternatives considered

| Option | Why rejected |
|---|---|
| No boundary; let features touch retrieval directly | Couples feature code to the hardest, most fragile part of the app |
| Move the sealed box to a separate assembly | Overkill for a single-project teaching app; the folder + interfaces + comments are enough |
