using Microsoft.Extensions.DependencyInjection;

namespace EventBooking.Modules.Payments;

public static class PaymentsModule
{
    public const string Schema = "payments";

    public static IServiceCollection AddPaymentsModule(this IServiceCollection services)
    {
        return services;
    }
}
