# ADR 0001: Modular monolith on .NET 10, EF Core 10 and PostgreSQL

- Status: Accepted
- Date: 2026-10-30

## Context

EventBooking is a learning project for an event ticketing system: publishing events, booking seats,
taking payments and notifying attendees. It should exercise real-world concerns (module boundaries,
transactional consistency, asynchronous processing, talking to an external payment provider) without
the operational cost of running many services.

## Decision

- Build a **modular monolith**. One deployable API (`EventBooking.Api`) hosts four modules, each a
  class library: `Events`, `Bookings`, `Payments`, `Notifications`.
- Each module owns its data in its **own PostgreSQL schema** (`events`, `bookings`, `payments`,
  `notifications`) through its own `DbContext`. Modules never query another module's tables.
- Modules talk to each other through public contracts (interfaces/integration events), not by
  referencing each other's internals.
- Background and asynchronous work (e.g. outbox dispatch, sending notifications, payment
  reconciliation) runs in a separate host, `EventBooking.Worker`, which reuses the module libraries.
- An external payment gateway is simulated by `EventBooking.FakePaymentProvider`, a separate minimal
  API, so the HTTP boundary, failures and webhooks are real.
- Stack: .NET 10 (LTS), ASP.NET Core minimal APIs, EF Core 10 with Npgsql, xUnit for unit and
  integration tests.

## Consequences

- Simple to run, debug and deploy: one database, two hosts plus the fake provider.
- Boundaries are enforced by convention and project references, not by the network; they need
  discipline (and later, architecture tests) to stay clean.
- A module can be extracted into its own service later because its data and contracts are already
  separate.
- A single PostgreSQL instance is a shared failure point, acceptable for this project.

## Alternatives considered

- **Microservices**: real isolation, but too much infrastructure for the current goals.
- **Single-project layered monolith**: least effort, but nothing stops modules from coupling.
