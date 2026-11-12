namespace EventBooking.Modules.Events.Endpoints;

// Every property is nullable on purpose. If Capacity were a plain int, a request that left it out
// would arrive as 0, and "missing" would look the same as "sent 0". Null means the client didn't send it.
internal sealed record CreateEventRequest(
    string? Name,
    string? Description,
    string? Venue,
    DateTime? StartsAtUtc,
    int? Capacity,
    decimal? Price);
