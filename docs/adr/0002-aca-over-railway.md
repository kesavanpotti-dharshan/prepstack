# 0002 — Azure Container Apps over Railway

## Status
Accepted

## Context
The API needs a host that runs a container, scales to zero to stay free for a
single-user personal tool, and deploys via OIDC without storing cloud credentials as
GitHub secrets. Railway is simpler to set up but has no free tier with scale-to-zero;
Azure Container Apps (Consumption plan) does, and pairs with Bicep for reviewable IaC —
both of which are worth having on a portfolio piece.

## Decision
Host the API on Azure Container Apps (Consumption), built from the same Dockerfile/image
regardless of host. Deploy via GitHub Actions → GHCR → ACA using OIDC (no stored
secrets). Keep the container image host-agnostic so Railway remains a drop-in fallback
if ACA's free grant or OIDC setup becomes a blocker.

## Consequences
- Cold start of a few seconds on the first request after idle, because of scale-to-zero.
  Acceptable for a personal tool; set `minReplicas: 1` only when demoing live.
- IaC lives in `infra/*.bicep`, reviewed like code, rather than configured by hand in the
  Azure portal.
- Secrets live in Container Apps secrets / Key Vault references, never in the repo or in
  GitHub Actions secrets (OIDC federation replaces long-lived credentials).
