# 0001: The modular monolith

## Reference (AI, checked against the docs)

### The modular monolith

I’ll start with the short comparison, because the module rules only make sense once you know what they protect. Then I’ll go deep on the two rules, using the code in your repo.

### Part A: The short version (monolith vs. microservices vs. modular monolith)

Keep two questions apart:

1. How many deployable units do you have? (one process, or many)
2. How strong are the walls inside the code?

| | Deployable units | Internal walls | Typical failure |
| --- | --- | --- | --- |
| Classic monolith | 1 | Weak or none. Any class can call any class or query any table. | A “big ball of mud”: everything is tangled, so changing one thing breaks something unrelated |
| Microservices | Many, each with its own database | Very strong. The network is the wall. | Network failures, distributed transactions, heavy ops cost. If the walls are drawn wrong, you get a distributed monolith: the tangle of a monolith plus the pain of a network. |
| Modular monolith | 1 | Strong, enforced by the compiler and by data ownership | The walls slowly decay if nobody enforces them |

The core idea: a modular monolith gives you microservice-style boundaries without microservice-style operations. You get one deploy, one process, in-memory calls and easy debugging. If one module later needs to scale on its own (say Payments), you can lift it out into a service, because it was never tangled with the rest.

The interview one-liner: “Microservices solve an organizational and scaling problem. Most teams don’t have that problem yet, but they all have a coupling problem. A modular monolith solves the coupling problem first and keeps extraction possible later.”

That’s all you need on the comparison. Now the important part.

### Part B (deep): Rule 1, modules don’t reference each other

#### What it means in your repo

Look at `EventBooking.Modules.Bookings.csproj`. It references EF Core and Npgsql, and no other module. Compare it with `EventBooking.Api.csproj:17-22`, which references all four modules.

That gives you this dependency graph:

```text
              ┌──────────── Api (host) ────────────┐
              │      Worker (host) also does this  │
              ▼          ▼            ▼            ▼
          Events     Bookings     Payments    Notifications
             ╳ ◄──────── ╳ ──────────► ╳ ◄──────── ╳      (no arrows between modules)
```

The hosts (Api, Worker) are the composition root: the single place where everything gets wired together. You can see it in `Program.cs:10-14`, where each module plugs itself in through `AddXxxModule()`. Modules know nothing about each other, and nothing about the host either.

#### Why the compiler is the best enforcer

If you write this inside Bookings:

```csharp
using EventBooking.Modules.Events;   // ❌ CS0246: namespace not found
```

the build fails. That matters a lot. Rules that rely on discipline (“please don’t call Events’ internals”) decay within months, especially under deadline pressure. A rule enforced by the compiler can’t decay quietly. Someone has to add a `ProjectReference` to break it, and that change shows up in code review.

#### Why the rule exists: what goes wrong without it

Imagine Bookings could reference Events directly:

```csharp
// Inside Bookings, in a world WITHOUT the rule
var ev = await eventsDbContext.Events.FindAsync(eventId);   // reads Events' tables
ev.AvailableSeats -= quantity;                              // changes Events' state
```

Three things are now broken:

- **Hidden coupling.** If Events renames `AvailableSeats` or changes how inventory works, Bookings breaks, and Events’ authors don’t even know Bookings depends on it.
- **Business rules are bypassed.** Events probably has rules (“can’t sell seats for a cancelled event”, “seats can’t go negative”). Bookings just skipped all of them.
- **Extraction becomes impossible.** You can’t move Events into its own service later, because Bookings reaches into its guts.

The rule forces each module to treat the others as a black box with a published API.

#### “But Bookings has to know about events!”

It does. A booking needs to know the event exists, isn’t cancelled and has seats. So modules communicate; they just don’t reference each other’s internals. There are three standard tools:

##### 1. Contracts projects (synchronous, in-process call)

Each module publishes a small, separate project that contains only interfaces and DTOs:

```text
EventBooking.Modules.Events.Contracts     ← tiny: interfaces + records, no logic
EventBooking.Modules.Events               ← implements the contract (internal stuff)
EventBooking.Modules.Bookings ──► Events.Contracts   (allowed: it only sees the "menu")
```

```csharp
// Events.Contracts: the public "menu" of the Events module
public interface IEventsModuleApi
{
    Task<EventSummary?> GetEventAsync(Guid eventId, CancellationToken ct);
}
public sealed record EventSummary(Guid Id, string Title, DateTimeOffset StartsAt, bool IsCancelled);
```

Bookings asks for `IEventsModuleApi` through DI. The host wires in the real implementation from inside Events. Bookings never sees `EventsDbContext` or the `Event` entity. It sees a stable, deliberately designed DTO.

Think of the contract as the restaurant menu and the module’s internals as the kitchen. Customers order from the menu and never walk into the kitchen.

##### 2. Integration events (asynchronous)

Some things are notifications rather than questions: “an event was cancelled”, “a booking was confirmed”. The owning module publishes a fact and other modules react:

```text
Events:  publishes EventCancelled { EventId }
Bookings: handles it → cancels the bookings for that event
Notifications: handles it → emails the attendees
```

Events doesn’t know who is listening. That’s the loosest coupling you can get. This is exactly where the outbox and Service Bus come in during Phase 4. For now, just know that this is the second tool.

##### 3. Local copies of data (read models)

If Bookings constantly needs an event’s title for display, it can keep its own small copy (`bookings.event_snapshots`), filled from integration events, so it doesn’t need to ask every time. You trade some freshness (eventual consistency) for independence.

> 📌 A decision ahead of you: your scaffold has no `.Contracts` projects yet. When the first module needs another one (very likely Bookings → Events for seats), you decide how they talk. That is a good candidate for ADR 0002, written by you. `CLAUDE.md` lists module boundaries and contracts as core learning.

#### Second-level enforcement: internal and architecture tests

- **`internal` by default.** Inside a module, entities, the DbContext and services should be `internal`. Only the `AddXxxModule()` entry point (and the contract implementations wired through DI) need to be public. The fewer public types a module has, the less there is to couple to.
- **Architecture tests.** Someone could still add a `ProjectReference` “just for a quick fix.” Libraries such as NetArchTest or ArchUnitNET let you write a test like “the Bookings assembly must not depend on the Events assembly” that fails in CI. It turns the rule into a red build.

#### Watch out for the “Shared” project

Every team eventually creates `EventBooking.Shared` or `Common`. It starts with a `Result` type, and a year later it holds entities, helpers and business logic, all coupled through the back door. Keep a shared kernel tiny and boring: primitives and cross-cutting abstractions only, never domain logic.

### Part C (deep): Rule 2, each module owns its schema

#### What a PostgreSQL schema is

A schema is a namespace inside one database. Your `architecture.md` sets up one database with four schemas:

```text
eventbooking (database)
├── events.        events, venues, seats ...
├── bookings.      bookings, reservations ...
├── payments.      payment_intents ...
└── notifications. outgoing_messages ...
```

So `events.events` and `bookings.bookings` are fully qualified table names. In the code, each module exposes its schema name, for example `EventsModule.cs:7`: `public const string Schema = "events";`.

#### What “owns” means: four concrete rules

1. **Only the owning module reads or writes its tables.** Bookings never queries `events.*`, not even read-only. It goes through the contract or keeps a local copy.
2. **One DbContext per module, mapped to its own schema.** When you build it, it will look roughly like this:

   ```csharp
   protected override void OnModelCreating(ModelBuilder b)
       => b.HasDefaultSchema(EventsModule.Schema);
   ```

   and with its own migrations history table:

   ```csharp
   options.UseNpgsql(conn, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", EventsModule.Schema));
   ```

   ⚠️ A classic pitfall: if you forget the history table, all four DbContexts share one `public.__EFMigrationsHistory`, and their migrations get mixed up with each other.

3. **No cross-schema joins.** Something like `SELECT ... FROM bookings.bookings JOIN events.events` silently re-couples the modules at the database level, even though the C# code looks clean.
4. **No foreign keys across schemas.** `bookings.bookings.event_id` is just a uuid column, with no FK to `events.events`.

#### Rule 4 has a real cost

Students often miss this. Dropping cross-module FKs means the database no longer guarantees that a booking points at a real event. You’ve moved that guarantee into the application:

- At write time, Bookings checks through the Events contract that the event exists.
- Afterwards, integration events keep things consistent: when an event is deleted or cancelled, Bookings reacts.

That’s a deliberate trade: you give up some database integrity in exchange for independence. In an interview, naming the cost is what separates a senior answer from a junior one.

#### What about transactions?

Two modules mean two DbContexts, which means no single transaction across them by default. Take “reserve seats in Events and create a booking in Bookings.” If the first commits and the second fails, you’re left in an inconsistent state.

This is the same problem microservices have, which is the point: your monolith is already honest about it. The solutions you’ll learn later are the outbox pattern, eventual consistency, and careful placement of the concurrency-critical write. Note that seat inventory lives in Events per your architecture doc, so the row-version check for overbooking belongs there. For now, just notice the problem exists.

#### Why schemas, not separate databases?

- **Cheaper and simpler:** one connection string, one server, one backup, one Docker container.
- **Still an exit path:** extracting Payments later means “move the payments schema to its own database.” Because nothing joins into it or has an FK to it, that’s a mechanical job.
- **Optional hard enforcement:** you could give each module its own PostgreSQL user, with `GRANT` rights on its schema only. Then a cross-schema query fails at runtime. Most teams don’t go that far, but it’s a good interview point.

#### A critical look at your current test

`ModuleSchemaTests.cs` only checks that the four schema names are different. It doesn’t check ownership: whether each DbContext really uses its schema, or whether any module touches another’s tables. That’s fine for a scaffold, but don’t mistake it for real enforcement. A good exercise later: once DbContexts exist, write a test that builds each module’s EF model and asserts that every entity maps to that module’s schema.

### Summary

| Rule | Enforced by | Protects against | Cost you accept |
| --- | --- | --- | --- |
| No module→module references | The compiler (missing `ProjectReference`), `internal`, architecture tests | Hidden coupling, bypassed business rules, no extraction path | You must design contracts or events for every interaction |
| Each module owns its schema | Conventions, one DbContext per module, optional DB permissions | Coupling through the database | No cross-module FKs or joins, no cross-module transactions, so eventual consistency |

### Interview angle

**Q: “Why did you choose a modular monolith?”**

A strong answer covers: you get boundaries without distributed-systems costs; the compiler enforces boundaries; data is split by schema so a module can be extracted later; and you name the trade-offs (no cross-module FKs or transactions, and you need contracts or events).

**Q: “How do your modules talk to each other?”**

Contracts for synchronous queries, integration events for facts, local read models when one module needs another’s data often. Never shared tables.
