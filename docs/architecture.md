# Architecture — Prepstack

## 1. Goals
- **Fast revision**: the core flow is "pick a topic → read questions (answers collapsed or expanded) → star what needs another look".
- **Content as code**: notes and curated questions can live in Git (`learnstack`) and sync in.
- **Portfolio-grade**: clean boundaries, tests, IaC, CI/CD, observability. Free-tier friendly.
- **Multi-tenant-ready**: single user at launch; every document carries `ownerId` so going SaaS is a config change, not a rewrite.

### Non-goals (v1)
Social features, real-time collaboration, mobile apps, payments.

## 2. High-level design
```
            ┌──────────────────────────┐
 Browser ──▶│ Cloudflare Pages (web)   │  app.<domain>
            │ React/Vite SPA           │
            └────────────┬─────────────┘
                         │ HTTPS (JWT bearer + httpOnly refresh cookie)
            ┌────────────▼─────────────┐
            │ Cloudflare proxy (DNS)   │  api.<domain>  — WAF, rate limit, TLS
            └────────────┬─────────────┘
            ┌────────────▼─────────────┐        ┌───────────────────────┐
            │ .NET 10 API              │───────▶│ MongoDB Atlas (M0)    │
            │ Azure Container Apps     │        │ + Atlas Search index  │
            │ (scale-to-zero)          │        └───────────────────────┘
            └──────┬──────────┬────────┘
                   │          │
       GitHub API ◀┘          └▶ LLM API (v2: generate questions)
   (learnstack sync)
```

### Stack decisions
| Concern | Choice | Why |
|---|---|---|
| Frontend host | Cloudflare Pages | Free, global edge, preview deploys per PR. New platform on the resume vs Vercel. |
| Frontend | React 19, Vite, TS, Tailwind, TanStack Query, React Router | Known stack, fast. |
| Markdown/code | react-markdown + rehype-shiki | Questions contain code; syntax highlighting matters. |
| API | .NET 10 Minimal APIs, Clean Architecture, CQRS-lite (handlers, no MediatR) | Core strength; fewer dependencies. |
| API host | Azure Container Apps (Consumption) | Containers + scale-to-zero + monthly free grant. Fallback: Railway (same image). |
| Database | MongoDB Atlas M0 | Questions are polymorphic documents (MCQ/code/open). Adds a document DB to a Postgres-heavy portfolio. |
| Search | Atlas Search (Lucene) | Full-text + facets without another service. |
| Auth | Own JWT (15 min) + rotating refresh token (httpOnly, `SameSite=Lax`, scoped to `.<domain>`) | Same-site because app and api share a registrable domain. Proven pattern. |
| IaC | Bicep | Azure-native, reviewable. |
| CI/CD | GitHub Actions → GHCR → ACA (OIDC, no stored secrets). Pages via Git integration. | |
| Observability | Serilog + OpenTelemetry → Azure Monitor / Grafana Cloud free | Traces across API → Mongo → LLM. |

**Known trade-off:** scale-to-zero means a cold start of a few seconds on first request. Acceptable for a personal tool; set `minReplicas: 1` only if demoing live.

## 3. Backend structure (Clean Architecture)
```
api/
  src/
    Prepstack.Domain/          Entities, value objects, domain errors. No dependencies.
    Prepstack.Application/     Commands/queries + handlers, validators (FluentValidation), ports (interfaces).
    Prepstack.Infrastructure/  Mongo (driver, class maps, indexes), GitHub client, LLM client, JWT/hash.
    Prepstack.Api/             Minimal API endpoints grouped by feature, auth, ProblemDetails, OpenAPI.
  tests/
    Prepstack.Domain.Tests/          Pure unit tests (invariants).
    Prepstack.Application.Tests/     Handlers with fakes for ports.
    Prepstack.Api.IntegrationTests/  WebApplicationFactory + Testcontainers MongoDB.
```
Feature folders inside Application/Api: `Topics`, `Questions`, `Study`, `Import`, `Auth`.
Dependency rule: Api → Application → Domain; Infrastructure → Application/Domain. Nothing depends on Api.

## 4. Data model (MongoDB)
All collections: `_id` (ObjectId), `ownerId`, `createdAt`, `updatedAt`. Soft-delete via `deletedAt` where noted.

**users**: `email` (unique), `passwordHash`, `displayName`, `roles[]`.

**refreshTokens**: `userId`, `tokenHash`, `familyId`, `expiresAt` (TTL index), `revokedAt`, `replacedBy`.

**topics**: `slug`, `name`, `parentId?`, `path` (materialized, e.g. `dotnet/di`), `description`, `visibility` (`private|public`), `questionCount` (denormalized).
- Index: `{ownerId:1, path:1}` unique.

**questions**
```jsonc
{
  "ownerId": "...",
  "topicIds": ["..."],
  "tags": ["di", "lifetimes"],
  "type": "flashcard | short | mcq | code | design",
  "difficulty": "easy | medium | hard",
  "prompt": "markdown",
  "options": [{ "id": "a", "text": "md", "correct": true }],   // mcq only
  "answer": "markdown",                                        // model answer
  "explanation": "markdown?",
  "source": { "kind": "manual | learnstack | ai", "externalId": "sha?", "path": "dotnet/di.yaml?", "url": "?" },
  "visibility": "private | public",
  "starred": false,                                           // flag for "revisit before interview"
  "version": 3,                                                // optimistic concurrency
  "deletedAt": null
}
```
- Indexes: `{ownerId:1, topicIds:1, difficulty:1}`, `{ownerId:1, "source.externalId":1}` unique sparse, Atlas Search index on `prompt, answer, tags`.
- Type-specific validation lives in the Domain (`Question.Create(...)` per type), not in the DB.


## 5. Study view
A read-optimized page for a topic (including subtopics) or any filter/search result.
- Toggle: answers collapsed (self-quiz while reading) or all expanded (fast skim).
- Keyboard: `j`/`k` next/previous, `space` reveal, `s` star. "Starred only" filter for last-minute revision.
- Print stylesheet so a topic can be saved as PDF for offline reading.
No scheduling or scoring state; reading is stateless apart from `starred`.

## 6. Learnstack import (content as code)
- Convention in the `learnstack` repo: `questions/<topic-path>/*.yaml`, each file a list of questions with a stable `id`.
- `POST /api/v1/import/learnstack` (manual "Sync" button; GitHub webhook in v2) pulls changed files via GitHub API.
- Upsert by `source.externalId = sha256(repo + path + id)`. Removed in Git → soft-deleted here.
- Imported questions are **read-only in the UI** (edit in Git). Review history is preserved across syncs.

## 7. API surface (v1, prefix `/api/v1`)
```
POST   /auth/register | /auth/login | /auth/refresh | /auth/logout
GET    /topics                      tree
POST   /topics   PATCH /topics/{id}   DELETE /topics/{id}
GET    /questions?topic=&tags=&difficulty=&type=&q=&cursor=&limit=
GET    /questions/{id}
POST   /questions   PUT /questions/{id} (If-Match: version)   DELETE /questions/{id}
PUT    /questions/{id}/star   DELETE /questions/{id}/star
GET    /study?topic=&includeSubtopics=&starred=   full questions for reading, ordered by topic path
POST   /import/learnstack
GET    /public/topics/{slug}         read-only shared decks (no auth)
-- v2 --
POST   /ai/questions:generate        { topicId, sourceMarkdown | learnstackPath, count }
```
Pagination: cursor-based (`_id`), max 100. Errors: ProblemDetails with `errors` for validation.

## 8. Frontend structure
```
web/src/
  app/          router, providers, layout
  features/     topics/ questions/ study/ auth/ import/
                each: api.ts (TanStack Query hooks), components/, pages/
  shared/       ui/ (buttons, inputs), markdown/, lib/ (http client, auth)
```
Key screens: Topic tree (home, with question counts) → Study view → Bank (filter/search/edit) → Public deck view.

## 9. Security
- Argon2id password hashing. Refresh token stored hashed; reuse detection revokes the whole family.
- Every repository method takes `ownerId`; there is no unscoped query path (enforced by tests).
- Rate limiting: ASP.NET Core rate limiter on auth + AI endpoints; Cloudflare rule as outer layer.
- CORS: allow only `app.<domain>` and Pages preview pattern.
- Secrets: Azure Container Apps secrets / Key Vault refs; never in repo. Atlas IP access list + least-privilege DB user.
- Markdown is rendered without raw HTML (`rehype-sanitize`).

## 10. Environments
| Env | Web | API | DB |
|---|---|---|---|
| local | Vite dev | `dotnet run` | docker Mongo 7 (replica set) |
| preview | Pages PR preview | shared dev ACA revision | Atlas `prepstack-dev` db |
| prod | Pages main | ACA prod | Atlas `prepstack` db |

## 11. Decision log
Record changes as ADRs in `docs/adr/NNNN-title.md` (context, decision, consequences).
Seed ADRs: 0001 MongoDB over Postgres · 0002 ACA over Railway · 0003 Read-only study mode, no spaced repetition · 0004 Git as source of truth for imported questions.
