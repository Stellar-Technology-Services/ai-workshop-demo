---
description: Generate focused xUnit tests for a C# service or class in this repo.
agent: agent
---

# Generate tests

Write xUnit tests for **${input:target:the class, service, or file to test}**.

## Context
- Tests live in `tests/ClaimsChat.Tests/` (xUnit, .NET 10, no mocking framework).
- Match the style of an existing test, for example [LexicalRankerTests.cs](../../tests/ClaimsChat.Tests/LexicalRankerTests.cs).
- Project conventions are in [copilot-instructions.md](../copilot-instructions.md).

## What to do
1. Read the target and list its public behavior and the edge cases worth covering.
2. Add a `<TypeName>Tests` class in `tests/ClaimsChat.Tests/`.
3. Cover the happy path plus edges: empty, null, boundary, and error conditions.
4. One behavior per test; arrange/act/assert; name tests `Method_Scenario_Expectation`.
5. Stay on public contracts; do not reach into `Services/SealedBox/` internals.
6. Run `dotnet test` and fix failures until it is green.

Keep tests deterministic: no real file, database, or network I/O.
