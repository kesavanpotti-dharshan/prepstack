# 0001 — MongoDB over Postgres

## Status
Accepted

## Context
Prepstack's core entity is the question: a polymorphic document whose shape depends on
`type` (`flashcard | short | mcq | code | design`). MCQ questions carry an `options[]`
array with embedded correctness flags; code questions carry language-tagged snippets;
other types don't need those fields at all. The rest of the portfolio already leans
relational/Postgres-heavy, so this project is also a chance to show a document database
used where it's the better fit, not just for novelty.

## Decision
Use MongoDB (Atlas M0 free tier) as the primary store. Each question is one document;
type-specific shape is validated in the Domain layer (`Question.Create(...)` per type),
not enforced by the database. Full-text search and faceting use Atlas Search instead of
a separate search service.

## Consequences
- No schema migrations for adding a new optional field to one question type — only a
  Domain-level validation change.
- Every query/update must filter by `ownerId` by convention (the database won't enforce
  this for us); see `docs/rules.md` → MongoDB.
- Multi-document writes (e.g. a learnstack sync batch) need a replica set for
  transactions — local dev runs Mongo as a single-node replica set for this reason.
- Losing relational joins is acceptable: topics/questions/users are looked up by id or
  simple filters, never joined server-side.
