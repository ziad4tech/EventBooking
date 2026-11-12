using EventBooking.Modules.Events.Persistence;
using Microsoft.AspNetCore.Http;

namespace EventBooking.Modules.Events.Endpoints;

internal static class EventEndpoints
{
    const decimal maxPrice = 9_999_999_999.99m; // Maximum price allowed (12 digits total, 2 decimal places)

    internal static async Task<IResult> CreateEventAsync(
        CreateEventRequest request,
        EventsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var newEvent = MapToEvent(request);

        dbContext.Events.Add(newEvent);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created($"/events/{newEvent.Id}", ToResponse(newEvent));
    }

    // Key: the field name. Value: every problem with that field. An empty dictionary means the request is valid.
    internal static Dictionary<string, string[]> Validate(CreateEventRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["Name"] = new[] { "Name is required." };
        }
        else if (request.Name.Trim().Length > 200)
        {
            errors["Name"] = new[] { "Name must be at most 200 characters long." };
        }

        if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Trim().Length > 2000)
        {
            errors["Description"] = new[] { "Description must be at most 2000 characters long." };
        }

        if (string.IsNullOrWhiteSpace(request.Venue))
        {
            errors["Venue"] = new[] { "Venue is required." };
        }
        else if (request.Venue.Trim().Length > 200)
        {
            errors["Venue"] = new[] { "Venue must be at most 200 characters long." };
        }

        if (request.StartsAtUtc == null)
        {
            errors["StartsAtUtc"] = new[] { "Start time is required." };
        }
        else
        {
            if (request.StartsAtUtc.Value.Kind != DateTimeKind.Utc)
            {
                errors["StartsAtUtc"] = new[] { "Start time must be in UTC." };
            }
            else if (request.StartsAtUtc <= DateTime.UtcNow)
            {
                errors["StartsAtUtc"] = new[] { "Start time must be in the future." };
            }
        }



        if (request.Capacity == null)
        {
            errors["Capacity"] = new[] { "Capacity is required." };
        }
        else
        {
            if (request.Capacity.Value <= 0)
            {
                errors["Capacity"] = new[] { "Capacity must be a positive number." };
            }
            else if (request.Capacity.Value > 100_000)
            {
                errors["Capacity"] = new[] { "Capacity can't exceed 100,000." };
            }
        }


        if (request.Price == null)
        {
            errors["Price"] = new[] { "Price is required." };
        }
        else
        {
            if (request.Price.Value < 0)
            {
                errors["Price"] = new[] { "Price cannot be negative." };
            }
            else if (request.Price.Value != Math.Round(request.Price.Value, 2))
            {
                errors["Price"] = new[] { "Price can have at most 2 decimal places." };
            }
            else if (request.Price.Value > maxPrice)
            {
                errors["Price"] = new[] { "Price cannot exceed 9,999,999,999.99." };
            }
        }

        return errors;
    }

    internal static Event MapToEvent(CreateEventRequest request)
    {
        var now = DateTime.UtcNow;
        var newEvent = new Event
        {
            Id = Guid.NewGuid(),
            Name = request.Name!.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description?.Trim()) ? null : request.Description!.Trim(),
            Venue = request.Venue!.Trim(),
            StartsAtUtc = request.StartsAtUtc!.Value,
            Capacity = request.Capacity!.Value,
            SeatsTaken = 0,
            Price = request.Price!.Value,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        return newEvent;
    }

    private static EventResponse ToResponse(Event source) => new(
        source.Id,
        source.Name,
        source.Description,
        source.Venue,
        source.StartsAtUtc,
        source.Capacity,
        source.SeatsTaken,
        source.Price,
        source.CreatedAtUtc,
        source.UpdatedAtUtc);
}
