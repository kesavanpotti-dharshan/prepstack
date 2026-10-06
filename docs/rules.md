# Rules — Prepstack

These are constraints, not suggestions. If a rule blocks a task, raise it; don't work around it.

## General
- Small vertical slices. A PR should be reviewable in under 10 minutes.
- Prefer boring, explicit code over clever abstractions. Delete before you add.
- Names describe intent: `GetDueQuestionsQuery`, not `QuestionService.Get2`.
- No commented-out code. No `TODO` without an issue link.

## Git
- Trunk-based: short-lived branches `feat/…`, `fix/…`, `chore/…`, `docs/…`. Squash merge.
- Conventional Commits with scope: `feat(api)`, `fix(web)`, `chore(infra)`, `docs`.
- `main` is always deployable. CI must be green before merge.

## .NET (api/)
- .NET 10, C# latest, `Nullable` enabled, `TreatWarningsAsErrors` true, `ImplicitUsings` on.
- Central package management (`Directory.Packages.props`); shared settings in `Directory.Build.props`.
- Minimal APIs grouped per feature (`MapGroup`). Endpoints are thin: bind → send to handler → map result.
- Handlers return `Result<T>` (no exceptions for expected failures). Exceptions = bugs; global handler → 500 ProblemDetails.
- Validation with FluentValidation in Application; domain invariants enforced in entity factories.
- `record` for DTOs/commands/queries. Domain entities have private setters and behavior methods.
- Async all the way; every async method accepts `CancellationToken`.
- Use `TimeProvider`, never `DateTime.UtcNow` directly.
- Logging: structured Serilog templates (`{QuestionId}`), never string interpolation. No PII or answers in logs.
- Options pattern with `ValidateOnStart()` for all config.

## MongoDB
- Every query and update filters by `ownerId`. No exceptions. Integration tests assert cross-tenant isolation.
- Use typed `Builders<T>` filters/updates; no raw BSON strings.
- Always project to what's needed for list endpoints; never return full documents for lists.
- No unbounded queries: list endpoints require a limit (default 20, max 100), cursor pagination.
- Updates use `version` for optimistic concurrency (`If-Match` → filter on version, `$inc` version).
- Indexes are declared in code (`MongoIndexInitializer`) and applied on startup; new query shape → new/verified index.
- Multi-document writes (e.g. a learnstack sync batch) use transactions; local Mongo runs as a replica set for this reason.
- Enums stored as strings. Dates stored as UTC.

## API conventions
- Prefix `/api/v1`. Plural nouns. Custom actions use `:verb` (`/ai/questions:generate`).
- Status codes: 200 read/update, 201 + `Location` create, 204 delete, 400 validation, 401/403 auth, 404, 409 version conflict, 429.
- All errors are ProblemDetails (`application/problem+json`) with a `traceId`.
- OpenAPI via built-in `Microsoft.AspNetCore.OpenApi` + Scalar UI (dev only).
- Breaking changes require `/v2`; additive changes don't.

## Frontend (web/)
- TypeScript `strict`, no `any` (use `unknown` + narrowing). ESLint + Prettier, zero warnings.
- Server state via TanStack Query only; no server data in global stores. Local UI state with `useState`/`useReducer`.
- API types generated from OpenAPI (`openapi-typescript`) — never hand-write response types.
- Feature-folder structure (see architecture.md §8). Shared UI in `shared/ui`; no cross-feature imports except via `shared`.
- Tailwind only; no inline style objects except dynamic values. Mobile-first, dark mode supported.
- Accessibility: keyboard-operable study flow, labeled inputs, visible focus, color isn't the only signal.
- Access token in memory only; refresh via httpOnly cookie. Never use localStorage for tokens.
- Render markdown with `rehype-sanitize`; no `dangerouslySetInnerHTML`.

## Testing
- Domain: unit tests for every rule (question-type invariants, topic paths). Fast, no I/O.
- Application: handler tests with in-memory fakes for ports.
- API: integration tests with `WebApplicationFactory` + Testcontainers MongoDB. Cover happy path, validation, auth, tenant isolation.
- Web: Vitest + Testing Library for components with logic; Playwright smoke test for login → open topic → reveal answer.
- Tests are named `Method_Scenario_Expected` (.NET) / `it("does X when Y")` (web).
- A bug fix starts with a failing test.

## Security
- No secrets in code, config files, or logs. `.env*` and `appsettings.*.local.json` are gitignored.
- Validate and size-limit all input (prompt/answer markdown ≤ 20 KB).
- AI endpoints: rate-limited, input length-capped, user content passed as data (never concatenated into system instructions).
- Dependabot on; CodeQL in CI.

## Dependencies
Add a package only if it saves meaningful code and is actively maintained. State the reason in the PR.
Pre-approved — api: MongoDB.Driver, FluentValidation, Serilog.*, OpenTelemetry.*, Scalar.AspNetCore, Konscious.Security.Cryptography (Argon2), xUnit, FluentAssertions alternative `Shouldly`, Testcontainers.MongoDb, NSubstitute.
Pre-approved — web: react-router, @tanstack/react-query, react-markdown, rehype-sanitize, shiki, zod, react-hook-form, openapi-typescript, vitest, @testing-library/react, playwright.
Not allowed without ADR: MediatR, AutoMapper, Redux/Zustand, ORMs/ODMs over the Mongo driver, UI kits beyond headless primitives.

## Docs
- Behavior or config change → update `architecture.md`. New decision → ADR.
- `README.md` stays recruiter-friendly: what it is, screenshot/GIF, architecture diagram, live link, how to run.
