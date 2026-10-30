using Microsoft.Extensions.DependencyInjection;

namespace EventBooking.Modules.Notifications;

public static class NotificationsModule
{
    public const string Schema = "notifications";

    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        return services;
    }
}
