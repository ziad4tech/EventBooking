using EventBooking.Modules.Events;
using EventBooking.Modules.Events.Endpoints;

namespace EventBooking.UnitTests;

public class EventResponseTests
{
    [Theory]
    [InlineData(100, 0, 100)]
    [InlineData(100, 30, 70)]
    [InlineData(100, 100, 0)]
    public void Available_seats_is_capacity_minus_seats_taken(int capacity, int seatsTaken, int expectedAvailable)
    {
        var source = new Event
        {
            Id = Guid.NewGuid(),
            Name = "Summer Concert",
            Venue = "City Hall",
            StartsAtUtc = DateTime.UtcNow.AddDays(1),
            Capacity = capacity,
            SeatsTaken = seatsTaken,
            Price = 25.50m,
        };

        var response = EventEndpoints.ToResponse(source);

        Assert.Equal(expectedAvailable, response.AvailableSeats);
    }
}
