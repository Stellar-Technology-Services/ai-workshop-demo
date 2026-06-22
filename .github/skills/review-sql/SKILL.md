---
name: review-sql
description: Reviews a SQL query, view, or stored procedure for correctness, performance, and safety before it ships. Use when reviewing, optimizing, debugging, or explaining a slow or risky SQL/T-SQL statement.
---

# review-sql

Acts as a senior database engineer reviewing SQL. The job is not to reformat the query, it is to find where it returns the wrong rows, scans when it could seek, or quietly corrupts data. Assume the database is bigger and busier than the author tested against.

Default to **T-SQL / SQL Server** semantics unless the dialect is obvious from the code. Flag where a rule is dialect-specific.

## Phase 1: Intent and shape

In 2-4 sentences, state what the query is meant to return or change, the tables and joins involved, and the assumptions it makes (cardinality, that rows exist, that a column is non-null). If an execution plan or row counts are available, read them; if not, say what you would need to confirm a performance claim.

*Done when:* intent and the data it touches are stated, and unknowns are named rather than assumed.

## Phase 2: Correctness

Hunt the bugs that still return a result set, so they pass a smoke test and fail in production:

- **NULL and three-valued logic.** `NOT IN (subquery)` returns nothing if the subquery yields a single NULL; `= NULL` is never true; aggregates skip NULLs; `COUNT(col)` ignores them. Prefer `NOT EXISTS`.
- **Join fan-out.** A one-to-many join multiplies rows and inflates `SUM`/`COUNT`. A `DISTINCT` bolted on to "fix" duplicates usually hides a wrong join.
- **Implicit conversion** that silently changes matching or comparison (`'01' = 1`, date string vs `datetime`, collation mismatches).
- **Non-deterministic paging/aggregation.** `TOP`/`OFFSET` or `GROUP BY` without a tie-broken, deterministic `ORDER BY` returns different rows on different runs.

*Done when:* every result-changing risk is named with the specific clause that causes it, or you state the query is correct and why.

## Phase 3: Performance

- **SARGability.** A function or calculation on an indexed column in `WHERE`/`JOIN` (`WHERE YEAR(created) = 2026`, `WHERE col + 0 = x`), a leading-wildcard `LIKE '%x'`, or an implicit conversion all defeat index seeks. Rewrite as a range (`>= '2026-01-01' AND < '2027-01-01'`).
- **`SELECT *`** drags unneeded columns and forces key lookups; project only what is used.
- **Set-based vs row-by-row.** Replace cursors/`WHILE` loops with a single set-based statement where possible.
- **Semi-joins.** Prefer `EXISTS` over `IN`/`COUNT(*) > 0` for existence checks; watch `OR` across columns that blocks index use (often better as `UNION`).
- **Indexes.** Name the index that *should* serve the predicate and join; flag missing, unused, or redundant ones. Judge against the **actual** plan, not the estimated one.

*Done when:* each performance call-out points at a specific clause and the index or rewrite that fixes it.

## Phase 4: Safety

- **Injection.** Any dynamic SQL must use `sp_executesql` with parameters, never string-concatenated input.
- **Destructive statements.** `UPDATE`/`DELETE` without a `WHERE`, or outside a transaction when multiple statements must succeed together. Confirm the blast radius is intended.
- **Concurrency and isolation.** `WITH (NOLOCK)` permits dirty reads; long transactions hold locks; missing transactions leave partial writes. Name the isolation assumption.
- **Hygiene.** Schema-qualify objects (`dbo.Foo`), `SET NOCOUNT ON` in procedures, explicit column lists on `INSERT`.

*Done when:* every injection, data-loss, and locking risk is listed, or the statement is confirmed read-only and safe.

## Operating rules

- Use the four phases in order; skip a phase only by saying it has no findings. Do not manufacture issues to look thorough.
- Quote the exact clause for each finding and give a concrete rewrite, not "consider optimizing."
- End with a one-line **Verdict**: `Ship`, `Ship with fixes`, `Rework`, or `Block`, plus a one-sentence reason.
