---
description: Hostile devil's-advocate review of the change you just made. Attacks the design, surfaces failure modes, and proposes hardening as reviewable diffs.
agent: agent
---

# Challenge

Act as a hostile reviewer and **devil's advocate** for the change that was just made.

Do **not** assume the current design is correct or sufficient. Your job is to **attack it** and surface problems, not to praise it.

## 0. Setup

- Run `git status` and `git diff` to see exactly which files changed in the latest work. Review only those changes and the code they touch.

## 1. Ruthless threat model

List the **top failure modes** and edge cases for this change, especially around:

- **Restarts, deploys, and migrations** — what breaks on app restart or when an EF migration runs against existing data?
- **Persistence and data integrity** — partial writes, change-tracking surprises, operations that should be one transaction but aren't.
- **Concurrency** — multiple users (or Blazor Server circuits) hitting the same data at once; anything assuming a single sequential caller.
- **Failure handling** — exceptions swallowed, errors logged but not surfaced, external/IO calls that time out or fail.
- **Edge-case data** — null, empty, missing, duplicated, or unexpectedly large input.

For each failure mode, answer: *How likely is this in a real system? What is the user-visible impact? What data do we lose or corrupt?*

## 2. Attack the assumptions

Explicitly **question the assumptions** in this design, including:

- That the data the code needs exists and is well-formed.
- That every database, file, or network operation succeeds.
- That callers arrive one at a time and in order.
- That the change is isolated and does not ripple into code paths the author didn't touch.

For each assumption, either argue **why it is unsafe**, or define the **strict conditions** under which it is acceptable and the tradeoff being chosen implicitly.

## 3. Concrete code-level critiques

- Point to **specific files and functions** (by name) that are risky or fragile.
- For each, explain **exactly why**: hidden coupling, mixed responsibilities in one method, silent failures, over-reliance on the happy path.
- Be explicit: "This line is dangerous because…" / "This method misbehaves if…"

## 4. Propose stronger alternatives (no code changes yet)

For the **highest-impact risks**, describe how you would redesign or harden the behavior (guards, retries, transactions, validation, narrower scope). For each, give the tradeoff (complexity, latency) and why it beats the current approach under real load and failure.

Respect the project's fixed boundaries (for example, the sealed box in `Services/SealedBox/`): harden around them, do not propose rewriting them.

## 5. Show hypothetical changes for review

For the top issues, sketch **hypothetical** code changes as **example blocks**, not final edits:

- Include the file path and method in a comment (e.g. `// src/ClaimsChat/Services/SomeService.cs`).
- Add brief comments explaining what each change solves.

The goal is concrete, reviewable diffs I can inspect before deciding to apply anything.

## 6. Final verdict

End with:

- A **short risk summary**: "If we ship this as-is, the biggest realistic risks are: …"
- A **priority list (3-5 items)** of the most important changes you recommend I consider first.
