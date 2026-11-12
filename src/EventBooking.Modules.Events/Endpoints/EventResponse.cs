namespace EventBooking.Modules.Events.Endpoints;

internal sealed record EventResponse(
    Guid Id,
    string Name,
    string? Description,
    string Venue,
    DateTime StartsAtUtc,
    int Capacity,
    int SeatsTaken,
    decimal Price,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
