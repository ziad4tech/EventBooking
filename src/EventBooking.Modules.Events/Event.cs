namespace EventBooking.Modules.Events;

internal sealed class Event
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public required string Venue { get; set; }

    public DateTime StartsAtUtc { get; set; }

    public int Capacity { get; set; }

    public int SeatsTaken { get; set; }

    public decimal Price { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
