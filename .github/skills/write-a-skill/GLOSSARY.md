# Glossary — write-a-skill

The domain model behind [`SKILL.md`](SKILL.md). A skill exists to wrangle determinism out of a stochastic model; every term below is a lever on that goal. Consult a term when the skill points at it — you do not need to read this top to bottom.

**Bold terms** in a definition are themselves defined here.

## Predictability

The degree to which a skill makes the agent behave the same *way* on every run — the same process, not the same output (a brainstorming skill should *predictably* diverge). The root virtue every other term serves.

## Model-Invoked

A skill that keeps its **description**, so the agent can fire it autonomously and other skills can reach it — the human can still type its name too. Pays a permanent **context load** in exchange for that discoverability. Choose it only when the agent or another skill must reach the skill on its own.

## User-Invoked

A skill with its **description** stripped (`disable-model-invocation: true`) — reachable only by the human typing its name. Zero **context load**, but it spends **cognitive load**: the human is the index that must remember it exists.

## Description

The skill's machine-readable trigger and the one pointer a **model-invoked** skill keeps loaded at all times. Its mere presence is the invocation axis: keep it and the skill is model-invoked; delete it and it is **user-invoked**. The source of a model-invoked skill's **context load**.

## Context Load

The cost a **model-invoked** skill imposes on the agent's context window — its **description**, always loaded, spending both tokens and attention. The brake on splitting into more model-invoked skills.

## Cognitive Load

The cost a **user-invoked** skill imposes on the human — remembering which skills exist and when to reach for each. Not a cost to minimize blindly: it is the price of human agency. The brake on splitting into more user-invoked skills.

## Information Hierarchy

A skill's content ranked by how immediately the agent needs it: **steps** (in-file, primary) → **reference** in-file (secondary) → reference disclosed behind a pointer. Keep the top legible; push down whatever you can.

## Steps

The ordered actions the agent performs — when a skill has them, the primary tier of its content. Every step ends on a **completion criterion**. Not every skill has steps: a skill can be all steps, all **reference**, or both.

## Completion Criterion

The condition that tells the agent a unit of work is done. Two properties make it a lever: *clarity* (can the agent tell done from not-done?) resists **premature completion**; *demand* (how much it requires, e.g. "every modified file accounted for") drives thorough work. The strongest criteria are both checkable and exhaustive.

## Reference

Material the agent refers to on demand — definitions, facts, rules, examples. Secondary to **steps** when a skill has them; the entire content when it has none. The prime candidate for **progressive disclosure**.

## Branch

A distinct case the skill handles — a situation that fires it and a path through it — so different runs take different routes. The unit that drives both invocation (one trigger per branch) and disclosure (inline what every branch needs, push down what only some reach). A linear skill has none; a many-cased one carries many.

## Progressive Disclosure

Moving **reference** out of `SKILL.md` and behind a pointer (a linked file in the skill folder), so the top stays legible. Inline what every run needs; disclose what only some runs reach. If a must-have pointer fires unreliably, sharpen its wording before pulling the material back inline.

## Leading Word

A compact concept already living in the model's pretraining that the agent thinks with while running the skill (e.g. *lesson*, *fog of war*, *tracer bullets*). Repeated as a token — never spelled out as a sentence — it accumulates a distributed definition and anchors a region of behavior in the fewest tokens. Reach for an existing word before coining your own; a made-up word recruits no priors.

## Single Source of Truth

Each meaning lives in exactly one authoritative place, so changing the behavior is a one-place edit. **Duplication** is its violation.

## Premature Completion

Ending a step before it is genuinely done, because attention slips to *being done*. A between-steps failure. Defense, in order: sharpen the **completion criterion** first (cheap, local); only if it is irreducibly fuzzy and you observe the rush, hide later steps by splitting the sequence.

## Duplication

The same meaning given more than one home. Costs maintenance and tokens, and inflates a meaning's prominence past its real rank. The accidental inverse of a **leading word**, which repeats a *token* on purpose, never the meaning.

## Sprawl

A skill simply too long, even when every line is live and unique. Cure it with the **information hierarchy**: disclose **reference** behind pointers and split by case.

## No-Op

A line the model already obeys by default — you pay load to say nothing. The test: does it change behavior versus the default? A **leading word** too weak to beat the default (*be thorough* when the agent already is) is a no-op; the fix is a stronger word, not a different technique.
