# CLAUDE.md — Prepstack (Interview Question Bank)

> Repo: `kesavanpotti-dharshan/prepstack`. Solution/project prefix: `Prepstack`.

## What this is
A personal-first, multi-tenant-ready question bank for interview prep and upskilling.
Questions are organized by topic, read in a focused Study view before interviews, and can be
imported from the `learnstack` notes repo. It is also a portfolio piece, so code quality,
tests, and docs are part of the product.

## Read before any task
1. `docs/architecture.md` — system design, data model, API surface. Source of truth.
2. `docs/rules.md` — coding, testing, security, git rules. Non-negotiable.
3. `docs/roadmap.md` — current phase and task list. Work only on the current phase.

If a task conflicts with these docs, stop and say so. Do not silently diverge.
If you change a design decision, update `docs/architecture.md` (and add an ADR in `docs/adr/`) in the same PR.

## Repo layout
```
/api        .NET 10 solution (Clean Architecture)
/web        React + Vite + TypeScript + Tailwind (Cloudflare Pages)
/docs       architecture.md, rules.md, roadmap.md, adr/
/infra      Bicep (Azure Container Apps) + docker-compose for local dev
/.github    workflows
```

## Commands
```bash
# local infra (MongoDB 7 replica set)
docker compose -f infra/docker-compose.yml up -d

# api
cd api && dotnet build && dotnet test
dotnet run --project src/Prepstack.Api            # http://localhost:5080, /scalar for API docs

# web
cd web && pnpm install && pnpm dev             # http://localhost:5173
pnpm lint && pnpm typecheck && pnpm test
```

## Workflow for every task
1. **Plan**: restate the goal, list files to touch, call out risks. Wait for approval on anything non-trivial.
2. **Implement** in small vertical slices (endpoint + handler + test + UI), not layer-by-layer.
3. **Verify**: build, lint, typecheck, and tests must pass. Run them; don't assume.
4. **Summarize**: what changed, what's left, any doc updates. Keep it short.
5. Commit with Conventional Commits (`feat(api): ...`). One logical change per commit.

## Definition of done
- Builds clean with zero warnings (`TreatWarningsAsErrors` is on).
- New behavior has tests (unit for domain/application, integration for API + Mongo).
- Every Mongo query is scoped by `ownerId` (see rules.md → Security).
- API errors return RFC 7807 ProblemDetails.
- No secrets, no `TODO` without an issue link, no dead code.
- Docs updated if behavior, config, or architecture changed.

## Don't
- Don't add packages without stating why and checking rules.md → Dependencies.
- Don't introduce new infrastructure (queues, caches, other DBs) without an ADR.
- Don't write repository-per-entity generic abstractions over the Mongo driver.
- Don't mock MongoDB in integration tests — use Testcontainers.
- Don't touch `infra/` or workflows unless the task is about them.
- Don't produce long explanations. Code + a brief summary.
