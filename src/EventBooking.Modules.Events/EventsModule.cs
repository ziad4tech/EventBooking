using EventBooking.Modules.Events.Endpoints;
using EventBooking.Modules.Events.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventBooking.Modules.Events;

public static class EventsModule
{
    public const string Schema = "events";

    public static IServiceCollection AddEventsModule(this IServiceCollection services)
    {
        services.AddDbContext<EventsDbContext>((serviceProvider, options) =>
            options.UseNpgsql(
                serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("EventBooking"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", Schema)));

        return services;
    }

    public static IEndpointRouteBuilder MapEventsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/events", EventEndpoints.CreateEventAsync);

        return endpoints;
    }

    public static async Task ApplyEventsMigrationsAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventsDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}
