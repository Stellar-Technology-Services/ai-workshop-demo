---
description: Turn a feature request into a bounded, reviewable implementation plan before any code is written.
agent: Plan
---

# Plan a feature

Produce a clear, scoped plan for **${input:feature:the feature or change to plan}**. Plan only; do not edit code.

## Steps
1. **Restate and clarify.** State the goal in one sentence. List your assumptions and ask any
   question whose answer would change the plan. Do not plan past a real ambiguity.
2. **Ground in the code.** Explore the relevant areas read-only so the plan fits how this project
   actually works, not how a generic project might.
3. **Decompose.** Break the work into ordered, independently reviewable steps. Name the files or
   areas each step touches.
4. **Define done.** State acceptance criteria and how each will be verified (tests, manual checks).
5. **Bound the scope.** List what is explicitly out of scope. Resist adjacent improvements and
   gold-plating; prefer the smallest change that satisfies the request.
6. **Surface risk.** Call out unknowns, risky areas, and any decision that needs a human call.

## Output
A written plan covering: goal, assumptions and open questions, ordered steps, acceptance criteria,
out-of-scope, and risks. No code changes in this pass.
