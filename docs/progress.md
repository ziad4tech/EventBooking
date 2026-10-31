# Progress

Tracker for the 24-week growth plan (10–12 hrs/week). Only move on when every "Done when" item of a
phase is checked. Falling behind is fine: extend the timeline, don't skip phases.

| Phase | Weeks | Status | Started | Finished | Notes |
| --- | --- | --- | --- | --- | --- |
| 1. Core API | 1–5 | In progress | 2026-10-30 | | |
| 2. Docker, CI/CD, Azure | 6–9 | Not started | | | |
| 3. Security | 10–12 | Not started | | | |
| 4. Distributed systems and observability | 13–16 | Not started | | | |
| 5. AI features | 17–20 | Not started | | | |
| 6. Hireable | 21–24 | Not started | | | |

## Phase 1: Core API (weeks 1–5)

### Build
- [x] Week 1: solution structure, Git repo, README, ADR 0001 (modular monolith)
- [ ] Week 1: first note in `docs/notes/`
- [ ] Week 2: Events module: CRUD, validation, pagination, filtering
- [ ] Week 3: Bookings module: reserve, confirm, cancel, capacity rules
- [ ] Week 3: Testcontainers set up (moved earlier from week 5, needed to prove week 4)
- [ ] Week 4: failing test that reproduces overbooking, *then* concurrency token and graceful
      `DbUpdateConcurrencyException` handling
- [ ] Week 4: global error handling (ProblemDetails) and structured logging
- [ ] Week 5: unit tests for business rules, integration tests against real PostgreSQL
- [ ] Week 5: load test (k6 or NBomber): 100 parallel bookings for the last 10 seats

### Done when
- [ ] The load test never overbooks
- [ ] All tests pass with one command
- [ ] I can explain async/await, including the .NET Framework deadlock, without notes
- [ ] Mock interview on Phase 1 done

## Phase 2: Docker, CI/CD, Azure (weeks 6–9)

### Build
- [ ] Azure budget alert set (before any deploy)
- [ ] Week 6: multi-stage Dockerfile, docker-compose with API + PostgreSQL
- [ ] Week 7: GitHub Actions builds and tests on every PR, fails on test failure
- [ ] Week 8: deploy to Azure Container Apps, database in Azure
- [ ] Week 8: secrets in Key Vault, managed identity (no connection-string passwords)
- [ ] Week 9: `/health/live` and `/health/ready` (with DB check), auto-deploy on merge to main
- [ ] Week 9: ADR: why Container Apps over App Service / AKS

### Done when
- [ ] Pushing to main deploys automatically
- [ ] A failing test blocks deployment
- [ ] No secrets in code or config files
- [ ] The app runs locally with a single `docker compose up`
- [ ] Mock interview on Phase 2 done

## Phase 3: Security (weeks 10–12)

### Build
- [ ] Week 10: API registered in Microsoft Entra ID, JWT bearer auth on endpoints
- [ ] Week 11: roles and policies (Admin manages events, Customer books), users see only their own bookings
- [ ] Week 11: client credentials flow for Worker → API
- [ ] Week 12: rate limiting (global + stricter on bookings), CORS, security headers
- [ ] Week 12: OWASP Top 10 self-review documented in `docs/notes/`

### Done when
- [ ] I can draw authorization code + PKCE from memory
- [ ] A test proves user A can't access user B's booking
- [ ] I can explain authN vs. authZ and OAuth2 vs. OIDC in one minute each
- [ ] Mock interview on Phase 3 done

## Phase 4: Distributed systems and observability (weeks 13–16)

### Build
- [ ] Week 13: Azure Service Bus: `BookingConfirmed` → Worker sends a confirmation
- [ ] Week 13: outbox table + background publisher, tested by killing the app mid-booking
- [ ] Week 14: fake payment provider fails 30% of the time and is randomly slow
- [ ] Week 14: resilience pipeline with `Microsoft.Extensions.Http.Resilience`
- [ ] Week 14: idempotency key on the payment endpoint (same key → same result, never charged twice)
- [ ] Week 15: Redis cache for event listings, invalidated on update, timings before and after
- [ ] Week 16: OpenTelemetry tracing API → Service Bus → Worker, exported to Application Insights
- [ ] Week 16: custom metrics (bookings/min, payment failures) and a failure-rate alert
- [ ] Week 16: ADRs: why outbox, caching strategy

### Done when
- [ ] Killing the payment provider mid-booking recovers with no lost or duplicated bookings
- [ ] One booking can be followed through the whole system in a single trace
- [ ] I can explain eventual consistency using the Notifications module
- [ ] Mock interview on Phase 4 done

## Phase 5: AI features (weeks 17–20)

### Build
- [ ] Week 17: `Microsoft.Extensions.AI` setup, basic `/assistant/ask` endpoint
- [ ] Week 18: embeddings for event descriptions and policies in pgvector (or Azure AI Search)
- [ ] Week 18: RAG pipeline that answers with sources
- [ ] Week 19: tool calling: `CheckAvailability(eventId)`, `ListEventsByDate(date)`
- [ ] Week 19: guardrails: refuses off-topic questions, never invents events, rate limited
- [ ] Week 20: evaluation set of 20 questions with expected answers, accuracy tracked
- [ ] Week 20: token usage and cost per request in telemetry

### Done when
- [ ] At least 85% accuracy on the 20 test questions
- [ ] It says "I don't know" instead of inventing events
- [ ] I can explain RAG vs. fine-tuning and when to use each
- [ ] Mock interview on Phase 5 done

## Phase 6: Hireable (weeks 21–24)

- [ ] README with architecture diagram, how to run, key decisions, live demo link
- [ ] `architecture.md` with component diagram and data flow
- [ ] At least 6 ADRs
- [ ] Clean commit history and a passing CI badge
- [ ] AI-200 exam booked (week 24 at the latest)
- [ ] System design practice: URL shortener, ticketing, chat/notifications, rate limiter
- [ ] 5 STAR stories: concurrency bug, resilience design, security decision, AI feature, Sitecore production issue
- [ ] CV rewritten, 3 posts (outbox, RAG in .NET, lessons from Sitecore), LinkedIn updated

## Weekly log

One line per week: what got built, what got learned, what's blocking.

| Week | Dates | Built | Learned / note | DDIA |
| --- | --- | --- | --- | --- |
| 1 | 2026-10-30 – | Solution scaffold, ADR 0001 | | |
