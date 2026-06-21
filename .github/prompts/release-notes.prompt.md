---
description: Draft developer-facing release notes from a range of git commits.
agent: agent
---

# Release notes

Summarize the commits in **${input:range:a git range, e.g. main..HEAD or v1.0..HEAD}** into release notes.

## What to do
1. Gather commits: `git log --no-merges --pretty=format:'%h %s' ${input:range}`.
2. Group changes under **Added**, **Changed**, **Fixed**, and **Other**. Drop empty groups.
3. Write one concise, user-facing bullet per meaningful change; fold trivial or noise commits together.
4. Lead with the most significant changes. Write for a developer reading a changelog.
5. Output markdown only. Do not invent changes that are not in the log.

If the range returns no commits, say so instead of guessing.
