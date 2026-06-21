---
name: quality-audit
description: Exhaustive read-only code-quality audit of recent changes — reports every check's findings and proposed fixes without editing code.
disable-model-invocation: true
---

# quality-audit

Runs a comprehensive, read-only **audit** over code that was just created or modified, plus the files those changes affect. Every check runs every time; findings are reported with a proposed fix, but no code is changed.

## Operating rules

- **Read-only** — never edit code. Relay findings and propose fixes; the human decides what to apply.
- **Exhaustive** — run all seven checks. Report "none found" for a clean check; never omit one.
- **Scope** — audit what was just created or modified and the files those changes touch, not the whole codebase, unless told otherwise.
- **Boundary** — this is hygiene and structure, not correctness. Defer logic bugs, edge cases, security, and merge verdicts to the `code-review` skill; do not duplicate its job.

## How to run

Go through the checks in order. For each:

1. State the check.
2. Show what you searched — patterns, globs, scope.
3. List findings as `path:line`, one per line. If clean, write "none found".
4. Propose the fix the check describes. Do not apply it.

End with a summary table of all seven checks, each marked **Found** (with a count) or **None found**. Ask the user before recommending sweeping changes that would touch many files.

*Done when:* all seven checks appear in the summary as Found or None found — none skipped, none collapsed together.

## Checks

1. **Duplicate utilities** — search the codebase for utility functions that duplicate or near-duplicate one another. *Propose:* consolidate to one implementation and point callers at it.
2. **Giant functions** — find functions over 400 lines. *Propose:* break into smaller single-responsibility functions with clear, descriptive names.
3. **Giant components** — find UI components over 200 lines. *Propose:* split data fetching, business logic, and presentation into separate layers.
4. **Dead code** — find functions and variables defined but never called or referenced. *Propose:* remove them; flag any that are ambiguous (public API, reflection, dependency injection, framework hooks) rather than assuming.
5. **Unsafe catches** — find catch blocks that are empty, silent, or broad/un-logged (e.g. `catch (Exception)` with no logging). *Propose:* add meaningful error logging and surface the failure to the caller or the user.
6. **API call states** — find API calls made from the UI. For each, verify a loading state, an error state, and a guard against duplicate requests on re-render. *Propose:* add whichever of the three is missing.
7. **Stale comments** — find comments whose neighbouring code has changed. *Propose:* rewrite any comment that no longer matches the code it sits next to; leave accurate ones untouched.
