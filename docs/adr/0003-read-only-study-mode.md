# 0003 — Read-only study mode, no spaced repetition

## Status
Accepted

## Context
The core use case is revising before an interview: skim or self-quiz a topic's questions
quickly, star the ones to revisit, move on. Spaced-repetition scheduling (SM-2 style
due dates, review grading) is a much bigger feature — scoring state per question per
user, a scheduler, review-session UX — and doesn't fit the pre-interview cramming flow
this tool is built for. It's explicitly a non-goal for v1.

## Decision
The Study view is stateless reading apart from a single boolean flag per question:
`starred`. No due dates, no scoring, no review history, no scheduling. The view just
orders questions by topic path, optionally filtered to `starred=true`, with
collapse/expand and keyboard navigation for a fast skim or self-quiz pass.

## Consequences
- `questions` documents only ever need `starred`, not a scheduling sub-document — keeps
  the data model and Study endpoint simple.
- No spaced-repetition algorithm, review grading UI, or due-date logic to design, test,
  or maintain.
- If spaced repetition is wanted later, it's a genuinely new feature (new state, new
  endpoints, new UI) layered on top — not a tweak to Study. Revisit this ADR if that
  becomes a goal.
