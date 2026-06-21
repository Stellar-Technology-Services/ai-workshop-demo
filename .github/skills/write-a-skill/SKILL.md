---
name: write-a-skill
description: Guides writing a new agent skill end to end — pinning its trigger, choosing invocation, writing the frontmatter, structuring the body, and pruning to a tight SKILL.md. Use when the user wants to create, write, or refine a skill.
---

# write-a-skill

Turns the agent into a skill author. A skill exists to wrangle **predictability** out of a stochastic model — the agent taking the same *process* every run. Every step below serves that goal.

**Bold terms** are defined in [`GLOSSARY.md`](GLOSSARY.md); consult it on demand, not all at once.
For the principles behind these steps (invocation, information hierarchy, leading words, failure
modes), see [`writing-great-skills.md`](writing-great-skills.md).

Work the steps in order. Each ends on its **completion criterion** — do not advance until it is met.

## 1. Pin the trigger and the job

State in one sentence what the agent becomes capable of. Then list the distinct cases the skill must handle — its **branches**. A branch is a situation that should fire the skill and a path through it; this list is the raw material for your triggers (step 3) and for what you disclose (step 4), so spend real effort here. If you cannot name a concrete trigger, stop — there is no skill here yet, just a wish.

*Done when:* the capability is one sentence and every distinct case the skill handles is named.

## 2. Choose invocation

- **Model-invoked** — keeps a `description`, so the agent fires it autonomously and other skills can reach it. Pays **context load**: the description sits in the window every turn.
- **User-invoked** — set `disable-model-invocation: true`; only the human typing its name can reach it. Zero context load, but the human must remember it exists (**cognitive load**).

Pick model-invoked only when the agent or another skill must reach it on its own. If it only ever fires by hand, make it user-invoked.

*Done when:* invocation is chosen and justified by who must reach the skill.

## 3. Write the frontmatter

`name` (kebab-case) and `description`, plus `disable-model-invocation: true` if you chose user-invoked (step 2). Drop any tool-specific fields.

Write the `description` for whoever reaches the skill:

- **Model-invoked** — it is the agent's trigger. Front-load the **leading word**; one trigger per **branch** (step 1), collapsing synonyms that rename the same branch; phrase triggers as the words a human actually types ("Use when the user wants…, mentions…"). Cut any identity already in the body.
- **User-invoked** — the agent never sees it; write a plain one-line summary for the human, no trigger list.

*Done when:* the frontmatter carries the right keys for the chosen invocation, and the description fits that invocation.

## 4. Structure the body

Decide what each piece is and how far down the **information hierarchy** it sits:

- **Steps** — ordered actions the agent performs. The primary tier; they earn their place in `SKILL.md`. End each on a **completion criterion**:
  - *Checkable* — the agent can tell done from not-done. "Understanding reached" fails the test; "every public method has a test" passes it.
  - *Exhaustive* where thoroughness matters — a criterion's demand is what drives the digging the agent does inside a step; slack here, and the agent does the minimum and moves on.
- **Reference** — facts, rules, definitions consulted on demand. Inline what every branch needs; push what only some branches reach into a co-located file behind a pointer (**progressive disclosure**) — exactly what this skill does with `GLOSSARY.md`.
- A skill may be all steps, all reference, or both.

Anchor behavior with a **leading word** — a concept already in the model's pretraining (e.g. *lesson*, *tracer bullets*, *fog of war*). Repeat it as a token, never as a sentence; it accumulates meaning and anchors a region of behavior in the fewest tokens.

*Done when:* every piece is placed as a step or reference, and each step has a completion criterion.

## 5. Prune

Hostile to every line. Cut, do not trim:

- **Single source of truth** — each meaning lives in exactly one place. Repeating it is **duplication**.
- **No-op** — a line the model already obeys by default changes nothing; delete it. (A weak leading word like *be thorough* is a no-op; the fix is a stronger word, not more words.)
- **Sprawl** — too long even when every line is live; disclose reference and split by **branch**.
- **Code blocks** — keep one only if the agent would get it wrong without the exact example. One per concept, under ~15 lines, no duplicate examples across frameworks.
- Target 60–120 lines; under 80 is ideal.

*Done when:* every surviving sentence changes behavior and earns its tokens.

## 6. Validate

- [ ] Frontmatter carries `name` and `description` (plus `disable-model-invocation` if user-invoked), no tool-specific fields
- [ ] Description fits the invocation — triggers-first if model-invoked, a plain one-liner if user-invoked
- [ ] Each step ends on a checkable completion criterion
- [ ] Walked one real case through the steps: every criterion holds, no step rushes to "done" (**premature completion**)
- [ ] Reference only some branches need is disclosed, not inlined
- [ ] No no-ops, duplication, or restated sections (recap, summary, rationale)
- [ ] Under 120 lines, code blocks pass the "would the agent get this wrong?" test
- [ ] File is named `SKILL.md` and lives in `skills/<skill-name>/`
