using Microsoft.Extensions.DependencyInjection;

namespace EventBooking.Modules.Bookings;

public static class BookingsModule
{
    public const string Schema = "bookings";

    public static IServiceCollection AddBookingsModule(this IServiceCollection services)
    {
        return services;
    }
}
