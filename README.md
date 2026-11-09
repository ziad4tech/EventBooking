# EventBooking

A learning project: an event booking system built as a .NET 10 modular monolith.

```bash
dotnet build
dotnet test
```

## Run locally

The API needs PostgreSQL. In Development it applies migrations on startup. Pick your own local password
and use it in both commands. It is stored in .NET user secrets, outside the repository.

```bash
docker run --name eventbooking-db -e POSTGRES_PASSWORD=YOUR_LOCAL_PASSWORD -e POSTGRES_DB=eventbooking -p 5432:5432 -d postgres:17
```

```bash
dotnet user-secrets set "ConnectionStrings:EventBooking" "Host=localhost;Port=5432;Database=eventbooking;Username=postgres;Password=YOUR_LOCAL_PASSWORD" --project src/EventBooking.Api
```

```bash
dotnet run --project src/EventBooking.Api
```

You don't need the next step to run the app. Only when you change the Events model, restore the
repository's EF tool once with `dotnet tool restore`, then add a migration named after your change:

```bash
dotnet ef migrations add YourChangeName --project src/EventBooking.Modules.Events --startup-project src/EventBooking.Api --output-dir Persistence/Migrations
```

See [docs/architecture.md](docs/architecture.md) and [docs/adr](docs/adr). Learning progress is
tracked in [docs/progress.md](docs/progress.md).
