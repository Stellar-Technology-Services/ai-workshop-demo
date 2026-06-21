---
name: explain-legacy
description: Explains unfamiliar or legacy code (what it does, how it flows, why, and where the risks are) before anyone changes it. Use when the user wants to understand, document, or safely prepare to modify code they did not write.
---

# explain-legacy

Turns the agent into a guide for code nobody on hand remembers. Read-only **orientation** first:
understand before you touch. Cite a concrete location for every claim, and mark anything you cannot
confirm as unknown rather than guessing.

## 1. Frame the target and the goal
Name what is being explained (file, module, flow) and why: pure understanding, documentation, or
preparing a change. The goal sets how deep to go.

*Done when:* the target and the reader's goal are stated in one sentence.

## 2. Trace the flow
Find the entry points and follow control and data through the target to its effects. Work from the
code itself, not from names or comments alone.

*Done when:* the path from entry to effect is traced with cited locations, not inferred from naming.

## 3. Recover intent
Explain why, not just what: the contract it fulfils, the invariants it assumes, the side effects it
causes, and any historical quirk or implicit assumption behind it.

*Done when:* every non-obvious behaviour has a stated reason or an explicit "unknown."

## 4. Locate the risk
Name what is dangerous to change here: hidden coupling, shared or global state, missing tests,
ordering dependencies, surprising side effects.

*Done when:* the change-risk hotspots are listed, each with the evidence that flags it.

## 5. Report
Deliver a concise briefing: what it does, how it flows, why, the risks, and (if a change is intended)
where to start and what to pin with characterization tests first. A developer who has never seen this
code should be able to orient from it.

*Done when:* the briefing covers behaviour, flow, intent, and risk, with cited locations.

## Boundary
This skill explains; it does not change. Hand off to a planning or implementation pass for the actual
edit, so comprehension and modification stay separate reviewable steps.