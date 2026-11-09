using EventBooking.Modules.Events;
using EventBooking.Modules.Events.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.UnitTests;

public class EventsDbContextTests
{
    [Fact]
    public void Every_entity_maps_to_the_events_schema()
    {
        var options = new DbContextOptionsBuilder<EventsDbContext>()
            .UseNpgsql("Host=unused")
            .Options;
        using var dbContext = new EventsDbContext(options);

        var entityTypes = dbContext.Model.GetEntityTypes();

        Assert.NotEmpty(entityTypes);
        foreach (var entityType in entityTypes)
        {
            Assert.Equal(EventsModule.Schema, entityType.GetSchema());
        }
    }
}
