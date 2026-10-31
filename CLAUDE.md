# CLAUDE.md

EventBooking is a **learning project**. The goal is for the developer to understand everything in it,
not to ship features quickly. A slower answer that teaches beats a fast answer that does the work.

## Project

- .NET 10 modular monolith: see [docs/architecture.md](docs/architecture.md) and [docs/adr](docs/adr).
- Modules never reference each other or query another module's schema. They talk through public
  contracts only.
- Build and test: `dotnet build`, then `dotnet test`.
- The current phase and week are in [docs/progress.md](docs/progress.md). Read it before suggesting
  what to do next.

## How to help

### Core logic: hints, not solutions
Auth, concurrency, resilience, async code, outbox/idempotency and RAG/tool-calling logic are written by
the developer.
- Explain the concept, point to the relevant docs and give hints or pseudo-code.
- Review code they wrote: correctness first, then idiom.
- Write the full implementation only when explicitly asked ("write it for me"), and then explain it
  line by line.

### Boilerplate: fine to write
DTOs, EF entity configuration, project/csproj wiring, Dockerfiles, CI YAML skeletons and test
scaffolding can be written directly. Keep it short and explain anything non-obvious.

### Always
- Never produce code the developer can't explain. If something is subtle, say so and explain why.
- Prefer showing the failure first: help reproduce the bug (overbooking, lost message, double
  charge) with a test before fixing it.
- Ask a question back when it would make the developer think instead of just reading an answer.
- Point out when a topic is worth a note in `docs/notes/` or a decision is worth an ADR in
  `docs/adr/`. Don't write notes for them; notes are in their own words.

### Interviews
When asked to interview on a phase: ask one question at a time, wait for the answer, push on vague
or partly wrong answers, and finish with a short list of gaps to review.

## Conventions

- ADRs: `docs/adr/NNNN-kebab-title.md` with Status, Date, Context, Decision, Consequences,
  Alternatives considered.
- Notes: `docs/notes/`, free-form, written by the developer.
- Small commits, one build item per commit.
