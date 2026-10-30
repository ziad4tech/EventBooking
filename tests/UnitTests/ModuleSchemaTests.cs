using EventBooking.Modules.Bookings;
using EventBooking.Modules.Events;
using EventBooking.Modules.Notifications;
using EventBooking.Modules.Payments;

namespace EventBooking.UnitTests;

public class ModuleSchemaTests
{
    [Fact]
    public void Each_module_owns_a_distinct_schema()
    {
        string[] schemas = [EventsModule.Schema, BookingsModule.Schema, PaymentsModule.Schema, NotificationsModule.Schema];

        Assert.Equal(schemas.Length, schemas.Distinct().Count());
    }
}
