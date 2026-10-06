# 0004 — Git as source of truth for imported questions

## Status
Accepted

## Context
Questions curated in the `learnstack` notes repo should be importable into Prepstack
without turning Prepstack into a second place to edit the same content. If imported
questions were editable in the UI, edits would silently diverge from the Git source and
the next sync would either clobber them or create merge-conflict-like ambiguity about
which copy is "right".

## Decision
Imported questions (`source.kind = "learnstack"`) are read-only in the Prepstack UI.
`POST /api/v1/import/learnstack` upserts by `source.externalId = sha256(repo + path +
id)`; a question removed from Git is soft-deleted (`deletedAt` set), never hard-deleted,
so review history survives across syncs. To change an imported question's content, edit
it in the `learnstack` repo and re-sync.

## Consequences
- The question editor must distinguish `source.kind` and disable editing for
  non-`manual` questions.
- Sync is idempotent and safe to re-run: same content in Git produces no-op updates.
- A learnstack sync batch is a multi-document write (upserts + soft-deletes together),
  so it runs in a transaction — which is why local Mongo must run as a replica set
  (see [0001](0001-mongodb-over-postgres.md)).
- v1 sync is manual (a "Sync" button); a GitHub webhook for automatic sync is v2 scope.
