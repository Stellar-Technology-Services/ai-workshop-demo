---
description: Scaffold a GitHub Actions CI workflow that builds and tests this .NET solution.
agent: agent
---

# Scaffold CI

Create or update `.github/workflows/ci.yml` to build and test this repo on CI.

## Requirements
- Trigger on `push` and `pull_request`.
- Use `actions/checkout@v5` and `actions/setup-dotnet@v5` with `global-json-file: global.json`
  so the pinned .NET 10 SDK is used.
- Steps: `dotnet build --configuration Release`, then `dotnet test --configuration Release --no-build`.
- Run on `windows-latest` (attendees clone on Windows). Keep it a single job.
- No secrets required; the app builds and tests without an AI key.

## Optional
- Add a boot smoke test that runs the app on a fixed `http://localhost` port and polls the homepage
  for a `200` before shutting it down.

Keep the workflow minimal and readable. If `ci.yml` already exists, update it rather than duplicating.
