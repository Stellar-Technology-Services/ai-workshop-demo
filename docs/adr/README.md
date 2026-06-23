# Architecture Decision Records

One file per decision. Files are never deleted — superseded ADRs are updated with a status of "Superseded by ADR NNN."

These ADRs document the decisions behind ClaimsChat as it stands today: a Blazor Server app with
a lexical-retrieval sealed box in front of Azure OpenAI. They double as workshop teaching material
for the "Security & Scaling across the SDLC" session.

## Index

| ADR | Title | Status |
|---|---|---|
| [001](001-sqlite-for-dev.md) | Use SQLite for development and test | Accepted |
| [002](002-sealed-box-boundary.md) | Retrieval + AI behind a sealed-box boundary | Accepted |
| [003](003-azure-openai-chat-backend.md) | Azure OpenAI as the chat backend (context-window grounding) | Accepted |
| [004](004-mcp-trust-boundaries.md) | MCP server configuration and trust boundaries | Accepted |
| [005](005-agent-definitions-and-a2a-trust.md) | Agent definitions and A2A trust rules | Accepted |

## Template

```markdown
# ADR NNN — <title>

**Status:** Proposed | Accepted | Superseded by ADR NNN
**Date:** YYYY-MM-DD
**Deciders:** <names or team>

## Context
<What is the problem or situation that forces a decision?>

## Decision
<What is the decision and the key reasoning?>

## Consequences
<What are the trade-offs? What becomes easier or harder?>

## Alternatives considered
| Option | Why rejected |
|---|---|
```
