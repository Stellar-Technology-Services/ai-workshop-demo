# ClaimsChat

A small demo app for the AI-assisted development workshop: a "chat with claims
documents" experience built in C# / .NET. See [`SPEC.md`](SPEC.md) for scope and
[`plan/tickets.md`](plan/tickets.md) for the build plan.

> This repo is a **teaching vehicle**, not a production app. The AI client and
> retrieval are a sealed box; the ordinary app around them is the teachable surface.

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
    "Endpoint": "https://ifi-workshop-resource.cognitiveservices.azure.com/",
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

## Project layout

```
ai-workshop-demo/
├─ global.json            # pins the .NET 10 SDK
├─ ClaimsChat.sln
├─ SPEC.md                # what we're building and why
├─ plan/tickets.md        # the build plan (T1..T6)
└─ src/ClaimsChat/        # the Blazor Server app
   ├─ Data/               # EF Core DbContext + migrations
   ├─ Services/SealedBox/ # AI client + retrieval (not exercise targets)
   └─ Components/         # Blazor pages, layout
```
