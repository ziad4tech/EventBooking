# Architecture

EventBooking is a modular monolith. See [ADR 0001](adr/0001-modular-monolith-on-dotnet-10.md).

## Projects

| Project | Kind | Purpose |
| --- | --- | --- |
| `src/EventBooking.Api` | ASP.NET Core web API | HTTP entry point; composes all modules |
| `src/EventBooking.Modules.Events` | Class library | Events, venues, seat inventory |
| `src/EventBooking.Modules.Bookings` | Class library | Reservations and bookings |
| `src/EventBooking.Modules.Payments` | Class library | Payment intents, calls to the provider |
| `src/EventBooking.Modules.Notifications` | Class library | Emails and other attendee notifications |
| `src/EventBooking.Worker` | Worker service | Background processing (outbox, notifications) |
| `src/EventBooking.FakePaymentProvider` | Minimal web API | Stand-in for an external payment gateway |
| `tests/UnitTests` | xUnit | Module logic tests |
| `tests/IntegrationTests` | xUnit | API tests via `WebApplicationFactory` |

## Dependencies

```
Api ──────► Modules.{Events, Bookings, Payments, Notifications}
Worker ───► Modules.{Events, Bookings, Payments, Notifications}
Modules ──► EF Core 10 + Npgsql (PostgreSQL)
Payments ─HTTP─► FakePaymentProvider
```

Modules do not reference each other. Each exposes an `Add<Module>Module()` extension that the hosts
call, and owns one PostgreSQL schema.

## Data

One PostgreSQL database, one schema per module: `events`, `bookings`, `payments`, `notifications`.
