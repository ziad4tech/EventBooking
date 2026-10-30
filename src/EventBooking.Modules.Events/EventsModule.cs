using Microsoft.Extensions.DependencyInjection;

namespace EventBooking.Modules.Events;

public static class EventsModule
{
    public const string Schema = "events";

    public static IServiceCollection AddEventsModule(this IServiceCollection services)
    {
        return services;
    }
}
