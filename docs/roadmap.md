# Roadmap — Prepstack

Claude Code works only on the **current phase**. Tick items as they merge.

**Current phase: 0**

## Phase 0 — Foundation
- [ ] Monorepo scaffold: `api/` solution (4 projects + 3 test projects), `web/` Vite app, `infra/docker-compose.yml` (Mongo 7 replica set)
- [ ] `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, ESLint/Prettier
- [ ] Health endpoint `/health`, Serilog, ProblemDetails, OpenAPI + Scalar
- [ ] Mongo connection, class maps, `MongoIndexInitializer`
- [ ] CI: build + test both apps on PR
- [ ] ADRs 0001–0004

## Phase 1 — Core bank (MVP, usable daily)
- [ ] Auth: register/login/refresh/logout with rotation + reuse detection
- [ ] Topics CRUD + tree
- [ ] Questions CRUD (flashcard, short, mcq, code) with markdown + code highlighting
- [ ] List/filter/paginate questions; Atlas Search full-text
- [ ] Web: auth pages, topic tree, bank list, question editor

## Phase 2 — Study view
- [ ] `/study` endpoint: topic + subtopics, starred filter
- [ ] Study page: collapse/expand answers, keyboard nav, star toggle
- [ ] Print stylesheet (save topic as PDF)

## Phase 3 — Ship it
- [ ] Bicep for ACA + secrets; GitHub Actions OIDC deploy (GHCR → ACA)
- [ ] Cloudflare Pages + custom domains `app.` / `api.`; CORS, cookie domain
- [ ] OpenTelemetry tracing; dashboards
- [ ] Playwright smoke in CI against preview
- [ ] README with GIF, diagram, live link → **add to resume/portfolio here**

## Phase 4 — Content as code
- [ ] YAML question schema + validator (publish JSON Schema for learnstack repo)
- [ ] Learnstack sync (manual), idempotent upsert, soft-delete removed
- [ ] Public read-only decks (`/public/topics/{slug}`) — shareable on LinkedIn

## Phase 5 — AI (differentiator)
- [ ] Generate questions from a learnstack note (review-before-save, never auto-publish)
- [ ] Cost guardrails: per-user daily cap, token logging

## Stretch
- GitHub webhook sync · Anki export · multi-user SaaS (billing) · MCP server exposing the question bank to Claude Desktop
