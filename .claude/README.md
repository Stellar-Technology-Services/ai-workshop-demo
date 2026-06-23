# Claude Code — Permission Configuration

This directory contains the shared Claude Code settings for the ClaimsChat workshop repo.
The settings demonstrate how to control which terminal commands the agent can run automatically
and which always require your explicit approval. They are a teaching artifact: read them
alongside [`.vscode/settings.json`](../.vscode/settings.json) (the GitHub Copilot equivalent)
and [`docs/adr/004-mcp-trust-boundaries.md`](../docs/adr/004-mcp-trust-boundaries.md).

---

## What `.claude/settings.json` controls

### `permissions.defaultMode`

The fallback behavior for any tool or command not matched by an explicit rule.

| Mode | Behavior |
|------|----------|
| `"default"` | Prompts for approval on first use of each tool |
| `"acceptEdits"` | Auto-approves file edits and safe filesystem ops (`mkdir`, `mv`, etc.) |
| `"plan"` | Read-only: Claude can explore but not edit or run commands |
| `"dontAsk"` | Denies everything not in the `allow` list |
| `"bypassPermissions"` | Skips all prompts — the "yolo" mode. Avoid in shared files. |

### `permissions.allow`

Commands and tools Claude can use **without prompting you**. Rules use the
`ToolName(specifier)` format with shell-style glob patterns:

```json
"Bash(dotnet test *)"   // matches any dotnet test invocation
"Read"                  // allows all file reads (no path restriction)
"Read(./src/**)"        // allows reads only under src/
```

### `permissions.deny`

Commands and tools that are **always blocked**, regardless of what any allow rule says.
A deny at any scope cannot be overridden — not by `allow`, not by `--allowedTools`, not
by a higher scope.

```json
"Bash(rm -rf *)"        // blocked no matter what
"Read(./src/ClaimsChat/appsettings.Development.json)"  // the agent cannot read your AI key
```

### Evaluation order

```
deny  →  ask  →  allow  →  defaultMode (fallback)
```

The first matching rule wins. `deny` rules always take precedence.

---

## Settings file locations and precedence

Claude Code merges settings from five sources, highest authority first:

| Priority | File | Scope |
|----------|------|-------|
| 1 (highest) | Managed settings (org-deployed) | Entire org — cannot be overridden |
| 2 | CLI flags (`--allowedTools`, `--disallowedTools`) | Current session only |
| 3 | `.claude/settings.local.json` | Your machine only — git-ignored |
| 4 | `.claude/settings.json` ← **this file** | Everyone who clones this repo |
| 5 (lowest) | `~/.claude/settings.json` | All your projects |

**Key rule:** if a tool is denied at *any* level, no lower level can un-deny it.
This means the shared project file (level 4) can tighten restrictions set at user level
(level 5), but cannot loosen restrictions set at managed level (level 1).

### Per-machine overrides with `.claude/settings.local.json`

`settings.local.json` is automatically added to `.gitignore` by Claude Code (this repo
also lists it explicitly). Use it for rules that only make sense on your machine:

```json
{
  "permissions": {
    "allow": [
      "Bash(dotnet ef database update *)"
    ]
  }
}
```

This does not affect your teammates' settings.

---

## What this repo's settings deny and why

| Rule | Reason |
|------|--------|
| `Bash(rm -rf *)` / `Bash(rm -r *)` | Irreversible recursive deletes |
| `Bash(curl *)` / `Bash(wget *)` | Network exfiltration; pulling untrusted content |
| `Bash(eval *)` / `Bash(exec *)` | Arbitrary code execution without visibility |
| `Bash(sudo *)` | Privilege escalation |
| `Bash(chmod *)` / `Bash(chown *)` | Filesystem permission changes |
| `Bash(git push --force*)` | Irreversible history rewrite on remote |
| `Bash(git reset --hard *)` | Discards uncommitted work silently |
| `Bash(dotnet ef database drop *)` | Destroys the local SQLite database during a live session |
| `Bash(dotnet nuget push *)` | Accidental package release |
| `Read(./src/ClaimsChat/appsettings.Development.json)` | Protects the Azure OpenAI key from appearing in context |
| `Read(./.env)` / `Read(./secrets/**)` | Protects secrets from appearing in context |
| `Read(~/.ssh/**)` etc. | Protects credentials outside the workspace |

Note that EF read-only commands (`dotnet ef migrations list` / `script`) are *allowed*, but
the destructive `dotnet ef database drop` is *denied* — the boundary is "inspect freely,
never destroy."

---

## MCP servers — where the config lives

Claude Code's MCP server definitions for this repo are in [`../.mcp.json`](../.mcp.json)
(repo root), **not** in this `.claude/` directory.

This is a hard constraint, not a style choice: Claude Code only auto-discovers
project-scoped MCP definitions from a `.mcp.json` file at the project root. It does **not**
read `.claude/mcp.json`, and `.claude/settings.json` has no `mcpServers` block — there is no
settings key, env var, or CLI flag to point it at a path inside `.claude/`. (Docs:
https://code.claude.com/docs/en/mcp-configuration.) `.mcp.json` must also be **strict JSON**
— no comments — which is why the explanation lives here instead of inline.

It is the Claude Code counterpart to [`.vscode/mcp.json`](../.vscode/mcp.json) (the GitHub
Copilot version) and is governed by the same trust boundaries in
[`docs/adr/004-mcp-trust-boundaries.md`](../docs/adr/004-mcp-trust-boundaries.md).

### The two servers

| Server | Transport | Purpose |
|--------|-----------|---------|
| `github` | HTTP (`api.githubcopilot.com/mcp/`) | Read issues / PRs / diffs to ground `/plan-feature` context |
| `sqlite` | stdio (`uvx mcp-server-sqlite`) | Inspect the local dev DB schema before generating migrations |

### How the read-only boundary is enforced

Unlike Copilot, Claude Code cannot use Copilot's silent OAuth, so `github` authenticates with
a **fine-grained, read-only PAT** supplied via the `GITHUB_PAT` environment variable (never
inline in the file). The boundary is enforced in two places:

1. **At the server** — `.mcp.json` sends `X-MCP-Readonly: true` and
   `X-MCP-Toolsets: repos,issues,pull_requests`, so write/DDL tools are never even exposed.
2. **At the agent layer** — `deny` rules in `settings.json` block the write tools
   (`mcp__github__create_pull_request`, `mcp__sqlite__write_query`, etc.) as belt-and-suspenders,
   per the ADR 004 principle of not trusting the server to self-limit.

### First-time setup

```bash
brew install uv          # provides uvx for the sqlite server
export GITHUB_PAT="github_pat_..."   # fine-grained token, Contents/Issues/PRs = Read-only
```

Then reload Claude Code, approve the project MCP servers when prompted, and run `/mcp` to
confirm both are connected. The `sqlite` DB path is absolute because Claude Code does not
expand `${workspaceFolder}`; the DB itself is created on the app's first run.

---

## Disabling bypass mode for your org (enterprise)

To prevent developers from using `--dangerously-skip-permissions` or
`bypassPermissions` mode across your entire organization, add a managed
settings file to your enterprise's private GitHub repository:

```json
{
  "permissions": {
    "disableBypassPermissionsMode": "disable"
  }
}
```

For Claude Code managed settings, see https://code.claude.com/docs/en/settings
(search for "managed settings").

---

## Quick reference: rule syntax

```
Bash(dotnet test)       exact command
Bash(dotnet test *)     command + any arguments
Bash(git log --)        up to a specific token
Read(./src/**)          path glob (** = any depth)
Read(~/.ssh/**)         absolute path glob
Edit(/src/**)           path glob for edits
WebFetch(domain:*.github.com)   domain-scoped fetch
WebSearch               entire tool (no specifier)
```

Docs: https://code.claude.com/docs/en/permissions
