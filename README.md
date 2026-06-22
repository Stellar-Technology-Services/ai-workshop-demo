# ClaimsChat

Materials for a hands-on **AI-assisted development workshop**. The repo is three things:

- **A demo app** — ClaimsChat, a "chat with claims documents" experience in C# / .NET
  (Blazor Server, EF Core + SQLite). See [`SPEC.md`](SPEC.md) for scope and
  [`plan/tickets.md`](plan/tickets.md) for the build plan.
- **A reusable AI-artifacts library** — repo instructions, prompts, skills, and a custom
  agent for GitHub Copilot, meant to be copied into your own stack. See
  [What's in here](#whats-in-here).
- **Slide decks** — the two presented sessions, as standalone HTML in [`workshop/`](workshop/).

> This repo is a **teaching vehicle**, not a production app. The AI client and
> retrieval are a sealed box; the ordinary app around them is the teachable surface.

## What's in here

The reusable AI artifacts are authored for **GitHub Copilot** but follow the open
[Agent Skills](https://agentskills.io/home) shape, so the bodies port to Cursor and
Claude Code with a different wrapper. See [`resources.md`](resources.md) for links.

- **[`.github/copilot-instructions.md`](.github/copilot-instructions.md)** — repo-wide
  instructions the agent loads on every request (stack, conventions, the sealed box).
- **[`.github/prompts/`](.github/prompts)** — reusable prompts you invoke as slash
  commands: `generate-tests`, `plan-feature`, `release-notes`, `scaffold-ci`, and
  `challenge` (a hostile devil's-advocate review of your latest change).
- **[`.github/skills/`](.github/skills)** — skills the agent loads when a task matches:
  `code-review`, `review-sql`, `explain-legacy`, `pushback`, `quality-audit`,
  `work-handoff`, `create-issue`, `depth-ui`, and `write-a-skill` (authoring new skills).
- **[`.github/agents/`](.github/agents)** — a custom `brownfield-discovery` agent for
  exploring an unfamiliar codebase read-only.
- **[`workshop/`](workshop)** — the two session decks as standalone HTML (open in a browser).
- **[`legacy/`](legacy)** — a standalone VB.NET snippet used as an explain/translate target.

## Workshop prerequisites

Install these **before the session** and do a dry run (see [Run it](#run-it)).

**Required**

1. **.NET 10 SDK** — the *SDK*, not just the runtime.
   - Windows: `winget install Microsoft.DotNet.SDK.10`, or the installer from the
     [official .NET download page](https://dotnet.microsoft.com/download/dotnet/10.0)
     (pick **x64** for most machines, **Arm64** for Arm devices).
   - Installs **side-by-side** with any existing .NET; user-local, usually no admin rights.
   - The repo pins .NET 10 via [`global.json`](global.json), so .NET 10 specifically is
     required — an older .NET alone won't satisfy it.
2. **Git** — to clone the repo (`winget install Git.Git` or Git for Windows).
3. **An editor** (any one):
   - **VS Code** + the **C# Dev Kit** extension (lightest), or **Cursor** — recommended.
   - **Visual Studio 2022 v17.14+** (the version that added .NET 10 support).
     ⚠️ An *older* VS 2022 will fail to build `net10.0` even with the SDK installed —
     use VS Code / the CLI instead if you can't update it.

**Verify**

```bash
dotnet --version    # must print 10.0.x
git --version
```

If `dotnet --version` shows an older version, your shell is resolving a different
.NET install — make sure the .NET 10 install is first on your `PATH`.

**Not needed** — no Docker, no SQL Server / LocalDB (file-based SQLite is bundled via
NuGet), no Node.js. The shared Azure OpenAI key is handed out **at the session**; the app
runs without it (the Chat page falls back to a stub model), so the dry run works key-free.

**Heads-up for locked-down machines:** the first run restores NuGet packages from
`api.nuget.org`. If a corporate proxy/firewall blocks it, restore fails — the dry run
surfaces this while there's still time to fix it.

## Run it

```bash
git clone <repo-url>
cd ai-workshop-demo
dotnet run --project src/ClaimsChat
```

Then open the URL printed in the console (e.g. `https://localhost:7xxx`).

The SQLite database is created automatically on first run — no manual database
step. **No AI key is required to boot:** with no key configured the Chat page falls
back to a stub model. Configure the key (below) to get real grounded answers.

## AI configuration

The chat model is **Azure OpenAI (gpt-5.4)**, reached via `Microsoft.Extensions.AI`.
Three values are needed: endpoint, deployment name, and key. They are supplied
**out-of-band at the workshop** and must **never be committed**.

To configure, copy the committed template to a gitignored file and fill it in:

```bash
cp src/ClaimsChat/appsettings.Development.example.json src/ClaimsChat/appsettings.Development.json
# then edit appsettings.Development.json and paste the values handed out at the session
```

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://<your-resource>.cognitiveservices.azure.com/",
    "Deployment": "<your-gpt-5.4-deployment-name>",
    "Key": "<paste-key-at-workshop-time>"
  }
}
```

- `appsettings.Development.json` is **gitignored** — your pasted key never lands in git.
- `Endpoint` is the **base host only** (no `/openai/...` path); the SDK adds the route.
- Prefer **.NET user-secrets**? `dotnet user-secrets set "AzureOpenAI:Key" "..." --project src/ClaimsChat`
  works too and overrides the file. Either way, nothing secret is committed.
- Leave the values blank and the app simply uses the stub (clone-and-run stays intact).

## Using the chat

The Chat page answers questions about the seeded claim documents. Lexical retrieval
ranks the documents for your question and feeds the **full text of the top few claims**
to the model, which streams back a markdown answer with **ranked source citations**.

- **Suggested-question chips** under the prompt are one-click starters, each phrased to
  retrieve a specific claim reliably.
- It answers about **individual claims** (cause of loss, damages, coverage, financials).
  It deliberately **cannot** total, count, or compare figures **across all claims** — ask
  about one claim at a time. Questions it can't answer get a short nudge toward ones it can.
- With no AI key configured the page falls back to a stub model (see [AI configuration](#ai-configuration)).

## Project layout

```
ai-workshop-demo/
├─ global.json            # pins the .NET 10 SDK
├─ ClaimsChat.sln
├─ SPEC.md                # what we're building and why
├─ resources.md           # links for going deeper / other harnesses
├─ .github/               # the reusable AI-artifacts library
│  ├─ copilot-instructions.md
│  ├─ prompts/            # slash-command prompts
│  ├─ skills/             # model/user-invoked skills
│  └─ agents/             # custom agents
├─ workshop/              # session slide decks (standalone HTML)
├─ legacy/                # VB.NET snippet (explain/translate target)
├─ plan/                  # build plan and tickets
├─ tests/ClaimsChat.Tests/ # xUnit tests
└─ src/ClaimsChat/        # the Blazor Server app
   ├─ Data/               # EF Core DbContext + migrations
   ├─ Services/SealedBox/ # AI client + retrieval (not exercise targets)
   └─ Components/         # Blazor pages, layout
```
