# Ticket: Exclude a document from chat retrieval

## Context

ClaimsChat answers questions grounded in the seeded claim documents. Sometimes a document should
not be used as a source: it is outdated, superseded, or simply noise. Today there is no way to take
a document out of the chat's sources short of deleting it from the folder.

## User story

As someone managing the document set, I want to mark a document as excluded from retrieval, so the
chat stops using it as a source while the document stays browsable in the app.

## Acceptance criteria

- Every document has an "included in retrieval" state that is **on by default**.
- The Documents view shows that state and lets me toggle it per document.
- An excluded document is **visibly marked** (a badge or similar).
- While excluded, the chat never retrieves or cites that document. Toggling it back on restores it.
- The choice **persists** across app restarts and across re-seeding.

## Constraints

- Do not modify the retrieval **ranking algorithm** or the AI client. Scoping *which* documents are
  eligible is fair game; changing *how* they are ranked is not.
- Bar is "legible and good enough," not production-grade. This is a small feature.
