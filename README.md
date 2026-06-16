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
NuGet), no Node.js. The shared Azure AI Foundry key is handed out **at the session** and
isn't required to run the current build.

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
step. **No AI key is required yet:** the current skeleton uses a stub chat model,
so the Chat page echoes your input. Real Azure AI Foundry chat is added in a later
ticket (see the plan).

## AI configuration (later tickets)

When the real chat model is wired up, the Azure AI Foundry key is supplied
out-of-band and stored in **.NET user-secrets** or a **gitignored**
`appsettings.Development.json` — **never committed**.

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
