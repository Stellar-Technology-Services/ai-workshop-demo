# ADR 004 — MCP Server Configuration and Trust Boundaries

**Status:** Accepted
**Date:** 2026-06-16
**Deciders:** Workshop team

---

## Context

The workshop adds `.vscode/mcp.json` with two MCP servers: GitHub (read-only) and SQLite
(local dev DB). This increases agent capability and attack surface. Trust boundaries must be
explicit before any MCP server is enabled.

MCP servers are the difference between an agent that **guesses** your context from code
(training data, pasted snippets) and one that **reads** the actual live state of your systems.
That grounding is the value — and the risk surface. MCP here is **developer tooling**: it serves
the agents and Copilot inside the editor. The ClaimsChat application itself does not speak MCP.

## Decision

### GitHub MCP

- **URL:** `https://api.githubcopilot.com/mcp/` (Copilot OAuth — no token in config)
- **Toolset scope:** `repos`, `issues`, `pull_requests` — read-only
- **Write tools disabled** — create PR, post comments, push commits are not in the toolset and
  therefore cannot be invoked even if an agent attempts to call them

**Use:** agents read open issues to populate `/plan-feature` context so the plan reflects actual
requirements, not a paraphrase the developer typed. Read-only.

### SQLite MCP

- **Command:** `uvx mcp-server-sqlite` (Python / uv required: `brew install uv`)
- **Database:** `${workspaceFolder}/src/ClaimsChat/ClaimsChat.db` — local dev DB only
- **Use:** agents inspect the live schema before generating a migration, or verify the seeded
  documents are present

## Trust boundary rules

| Action | Allowed without approval | Requires approval | Never |
|---|---|---|---|
| Read GitHub issues | ✅ | | |
| Read GitHub diffs / PRs | ✅ | | |
| Post PR comments | | | ❌ |
| Query schema (PRAGMA, sqlite_master) | | ✅ prompt per call | |
| Run SELECT on ClaimsChat.db | | ✅ prompt per call | |
| INSERT / UPDATE / DELETE | | | ❌ |
| DDL (CREATE / DROP / ALTER) | | | ❌ |
| Point at a production database | | | ❌ |

Write and DDL operations are blocked at the agent permission layer (`.vscode/settings.json` and
`.claude/settings.json`), not by trusting the MCP server to self-limit.

## Application-layer boundaries still apply

MCP access to the database does NOT override architectural rules. Specifically:

- The sealed-box boundary (ADR 002) is an application constraint, not a database constraint —
  MCP schema inspection does not grant permission to rewrite the ranking/prompt/AI call.
- The `IncludedInRetrieval` flag on `Document` is the data-layer control for what enters the
  model's context. Toggling it via raw SQL through the MCP server bypasses the application layer
  and is not permitted — **use the Documents page**, not raw `UPDATE`. (DML is denied anyway, but
  the point stands: the UI is the supported control, not the database.)

## Consequences

- Agents are grounded in live state (schema, issues) rather than guessing.
- Scope creep is bounded: write tools are simply not offered in the MCP configuration.
- `uvx mcp-server-sqlite` requires Python/uv on the developer machine — document in the README.
- The DB file does not exist until the app has been run once (it is created and seeded on first
  launch); inspect the schema after a `dotnet run`.
