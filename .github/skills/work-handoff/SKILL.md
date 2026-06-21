---
name: work-handoff
description: Hands off finished work as a single self-contained, shareable HTML file — what changed, why (decision flow), diagrams, and what was left out of scope. Use when a dev finishes a piece of work (a fix, a feature, a PR for review) and wants to write it up or share it with other devs.
---

# work-handoff

Turns finished work into a **handoff**: one self-contained HTML file a dev can pass to another dev who then understands what was done, why, and what was deliberately left out — without a walkthrough.

## What it produces

A single `.html` file with these sections, in order:

- **Summary** — what the work accomplishes in 2–3 sentences; link the ticket or PR if there is one.
- **What changed** — the concrete changes grouped by area, each citing the files touched.
- **Why / decisions** — the path taken and the alternatives rejected, so the reader inherits the reasoning, not just the result.
- **Diagrams** — data or control flow that aids understanding (see HTML rules).
- **Out of scope** — what was intentionally left out or deferred, so the reader never mistakes a deliberate omission for a gap.
- **Follow-ups** — open questions or next steps, if any.

## Steps

### 1. Gather the work

Assemble the change set from git — branch-vs-main diff, or staged/unstaged plus recent commits — together with the session context. Establish what changed, where, and why.

*Done when:* every changed file is accounted for in the gathered set — none dropped.

### 2. Write the overview

Fill each section above from the gathered work. Write for a dev who was not here: expand acronyms, name the files, and state the reasoning behind every non-obvious decision. Include a diagram only when it carries a real flow; skip decorative ones.

*Done when:* every section is present (or explicitly marked "none"), and each decision states its reasoning.

### 3. Render and save

Emit one self-contained HTML file and save it to `handoffs/<yyyy-mm-dd>-<slug>.html`. Report the path so the dev can share it.

*Done when:* the file opens standalone with no missing assets, and the saved path is reported.

## HTML rules

- **Self-contained** — one file, all CSS in a single inline `<style>`, no external stylesheets, fonts, or CDN scripts. It must render fully offline.
- **Readable** — system font stack, a max-width text column, clear headings, muted captions, print-friendly.
- **Diagrams as inline SVG** — no diagram libraries. Use labeled boxes joined by directional arrows; reuse this pattern:

```html
<svg viewBox="0 0 520 84" role="img" aria-label="Data flow">
  <defs><marker id="arr" markerWidth="8" markerHeight="8" refX="6" refY="3" orient="auto">
    <path d="M0,0 L6,3 L0,6 Z" fill="#556"/></marker></defs>
  <g font-family="sans-serif" font-size="14" text-anchor="middle">
    <rect x="10" y="26" width="120" height="34" rx="6" fill="#eef" stroke="#557"/><text x="70" y="48">Request</text>
    <line x1="130" y1="43" x2="196" y2="43" stroke="#556" marker-end="url(#arr)"/>
    <rect x="200" y="26" width="120" height="34" rx="6" fill="#eef" stroke="#557"/><text x="260" y="48">Service</text>
    <line x1="320" y1="43" x2="386" y2="43" stroke="#556" marker-end="url(#arr)"/>
    <rect x="390" y="26" width="120" height="34" rx="6" fill="#eef" stroke="#557"/><text x="450" y="48">Response</text>
  </g>
</svg>
```

## Boundary

This explains and visualizes finished work; it does not judge it. For correctness use `code-review`; for hygiene use `quality-audit`.
