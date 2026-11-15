using EventBooking.Modules.Events;
using EventBooking.Modules.Events.Endpoints;

namespace EventBooking.UnitTests;

public class EventUpdateTests
{
    private static readonly DateTime CreatedLongAgo = DateTime.UtcNow.AddDays(-5);

    // An event as it might look in the database before the update.
    private static Event ExistingEvent() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Old Name",
        Description = "Old description",
        Venue = "Old Venue",
        StartsAtUtc = DateTime.UtcNow.AddDays(10),
        Capacity = 50,
        SeatsTaken = 30,
        Price = 10m,
        CreatedAtUtc = CreatedLongAgo,
        UpdatedAtUtc = CreatedLongAgo,
    };

    // A valid PUT body where every editable field differs from ExistingEvent().
    private static CreateEventRequest UpdateRequest() => new(
        Name: "New Name",
        Description: "New description",
        Venue: "New Venue",
        StartsAtUtc: DateTime.UtcNow.AddDays(20),
        Capacity: 80,
        Price: 15.75m);

    // ---------- ApplyUpdate ----------

    [Fact]
    public void Update_copies_every_editable_field()
    {
        var target = ExistingEvent();
        var request = UpdateRequest();

        EventEndpoints.ApplyUpdate(target, request);

        Assert.Equal("New Name", target.Name);
        Assert.Equal("New description", target.Description);
        Assert.Equal("New Venue", target.Venue);
        Assert.Equal(request.StartsAtUtc, target.StartsAtUtc);
        Assert.Equal(80, target.Capacity);
        Assert.Equal(15.75m, target.Price);
    }

    [Fact]
    public void Update_never_changes_id_seats_taken_or_created_time()
    {
        var target = ExistingEvent();
        var originalId = target.Id;

        EventEndpoints.ApplyUpdate(target, UpdateRequest());

        Assert.Equal(originalId, target.Id);
        Assert.Equal(30, target.SeatsTaken);
        Assert.Equal(CreatedLongAgo, target.CreatedAtUtc);
    }

    [Fact]
    public void Update_sets_updated_time_to_now()
    {
        var target = ExistingEvent();
        var before = DateTime.UtcNow;

        EventEndpoints.ApplyUpdate(target, UpdateRequest());

        Assert.InRange(target.UpdatedAtUtc, before, DateTime.UtcNow);
    }

    [Fact]
    public void Update_trims_text()
    {
        var target = ExistingEvent();
        var request = UpdateRequest() with
        {
            Name = "  New Name  ",
            Description = "  New description  ",
            Venue = "  New Venue  ",
        };

        EventEndpoints.ApplyUpdate(target, request);

        Assert.Equal("New Name", target.Name);
        Assert.Equal("New description", target.Description);
        Assert.Equal("New Venue", target.Venue);
    }

    // Same rule as creating an event: "no description" is always stored as null.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_with_missing_or_blank_description_stores_null(string? description)
    {
        var target = ExistingEvent();
        var request = UpdateRequest() with { Description = description };

        EventEndpoints.ApplyUpdate(target, request);

        Assert.Null(target.Description);
    }

    // ---------- Capacity rule (409) ----------

    [Theory]
    [InlineData(29, true)]   // one below the seats already taken: conflict
    [InlineData(30, false)]  // exactly the seats taken: allowed ("never below")
    [InlineData(31, false)]
    public void Capacity_below_seats_taken_is_a_conflict(int newCapacity, bool expectedConflict)
    {
        var existing = ExistingEvent(); // SeatsTaken = 30
        var request = UpdateRequest() with { Capacity = newCapacity };

        var conflict = EventEndpoints.IsCapacityBelowSeatsTaken(existing, request);

        Assert.Equal(expectedConflict, conflict);
    }

    // ---------- Creating an event (US-04) with no description ----------

    [Fact]
    public void Create_mapping_with_missing_description_stores_null()
    {
        var request = new CreateEventRequest(
            Name: "Summer Concert",
            Description: null,
            Venue: "City Hall",
            StartsAtUtc: DateTime.UtcNow.AddDays(1),
            Capacity: 100,
            Price: 25.50m);

        var mapped = EventEndpoints.MapToEvent(request);

        Assert.Null(mapped.Description);
    }
}
