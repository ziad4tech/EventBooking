using EventBooking.Modules.Events.Endpoints;

namespace EventBooking.UnitTests;

public class EventsValidationTests
{
    private static CreateEventRequest ValidRequest() => new(
        Name: "Summer Concert",
        Description: "An evening of live music",
        Venue: "City Hall",
        StartsAtUtc: DateTime.UtcNow.AddDays(1),
        Capacity: 100,
        Price: 25.50m);

    private static CreateEventRequest EmptyRequest() => new(
        Name: null,
        Description: null,
        Venue: null,
        StartsAtUtc: null,
        Capacity: null,
        Price: null);

    // ---------- The request as a whole ----------

    [Fact]
    public void Valid_request_has_no_errors()
    {
        var request = ValidRequest();

        var errors = EventEndpoints.Validate(request);

        Assert.Empty(errors);
    }

    [Fact]
    public void Empty_request_has_errors()
    {
        var request = EmptyRequest();

        var errors = EventEndpoints.Validate(request);

        Assert.Contains("Name", errors.Keys);
        Assert.Contains("Venue", errors.Keys);
        Assert.Contains("StartsAtUtc", errors.Keys);
        Assert.Contains("Capacity", errors.Keys);
        Assert.Contains("Price", errors.Keys);
        Assert.Equal(5, errors.Count);
    }

    [Fact]
    public void Every_invalid_field_is_reported_not_only_the_first()
    {
        var request = new CreateEventRequest(
            Name: "",
            Description: new string('d', 2001),
            Venue: " ",
            StartsAtUtc: DateTime.UtcNow.AddDays(-1),
            Capacity: 0,
            Price: -1m);

        var errors = EventEndpoints.Validate(request);

        Assert.Equal(6, errors.Count);
    }

    // ---------- Name ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_or_blank_name_is_rejected(string? name)
    {
        var request = ValidRequest() with { Name = name };

        var errors = EventEndpoints.Validate(request);

        Assert.Contains("Name", errors.Keys);
        Assert.Single(errors);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(200, true)]
    [InlineData(201, false)]
    public void Name_must_be_1_to_200_characters(int length, bool expectedValid)
    {
        var request = ValidRequest() with { Name = new string('a', length) };

        var errors = EventEndpoints.Validate(request);

        Assert.Equal(expectedValid, !errors.ContainsKey("Name"));
    }

    [Fact]
    public void Name_length_ignores_surrounding_spaces()
    {
        var request = ValidRequest() with { Name = new string('a', 195) + new string(' ', 10) };

        var errors = EventEndpoints.Validate(request);

        Assert.Empty(errors);
    }

    // ---------- Description ----------

    [Fact]
    public void Missing_description_is_allowed()
    {
        var request = ValidRequest() with { Description = null };

        var errors = EventEndpoints.Validate(request);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(2000, true)]
    [InlineData(2001, false)]
    public void Description_must_be_at_most_2000_characters(int length, bool expectedValid)
    {
        var request = ValidRequest() with { Description = new string('d', length) };

        var errors = EventEndpoints.Validate(request);

        Assert.Equal(expectedValid, !errors.ContainsKey("Description"));
    }

    // ---------- Venue ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_or_blank_venue_is_rejected(string? venue)
    {
        var request = ValidRequest() with { Venue = venue };

        var errors = EventEndpoints.Validate(request);

        Assert.Contains("Venue", errors.Keys);
        Assert.Single(errors);
    }

    [Theory]
    [InlineData(200, true)]
    [InlineData(201, false)]
    public void Venue_must_be_at_most_200_characters(int length, bool expectedValid)
    {
        var request = ValidRequest() with { Venue = new string('v', length) };

        var errors = EventEndpoints.Validate(request);

        Assert.Equal(expectedValid, !errors.ContainsKey("Venue"));
    }

    // ---------- StartsAtUtc ----------

    [Fact]
    public void Missing_start_time_is_rejected()
    {
        var request = ValidRequest() with { StartsAtUtc = null };

        var errors = EventEndpoints.Validate(request);

        Assert.Contains("StartsAtUtc", errors.Keys);
        Assert.Single(errors);
    }

    [Fact]
    public void Start_time_in_the_past_is_rejected()
    {
        var request = ValidRequest() with { StartsAtUtc = DateTime.UtcNow.AddDays(-1) };

        var errors = EventEndpoints.Validate(request);

        Assert.Contains("StartsAtUtc", errors.Keys);
    }

    [Fact]
    public void Start_time_without_a_time_zone_is_rejected()
    {
        // What "2027-01-10T18:00:00" (no Z, no offset) becomes after the JSON is read.
        var noTimeZone = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(1), DateTimeKind.Unspecified);
        var request = ValidRequest() with { StartsAtUtc = noTimeZone };

        var errors = EventEndpoints.Validate(request);

        Assert.Contains("StartsAtUtc", errors.Keys);
    }

    [Fact]
    public void Start_time_in_local_time_is_rejected()
    {
        // What "2027-01-10T18:00:00+02:00" becomes after the JSON is read.
        var localTime = DateTime.Now.AddDays(1);
        var request = ValidRequest() with { StartsAtUtc = localTime };

        var errors = EventEndpoints.Validate(request);

        Assert.Contains("StartsAtUtc", errors.Keys);
    }

    // ---------- Capacity ----------

    [Fact]
    public void Missing_capacity_is_the_only_error()
    {
        var request = ValidRequest() with
        {
            Capacity = null,
        };

        var errors = EventEndpoints.Validate(request);

        Assert.Single(errors);
        Assert.Contains("Capacity", errors.Keys);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(100_000, true)]
    [InlineData(100_001, false)]
    public void Capacity_must_be_1_to_100000(int capacity, bool expectedValid)
    {
        var request = ValidRequest() with { Capacity = capacity };

        var errors = EventEndpoints.Validate(request);

        Assert.Equal(expectedValid, !errors.ContainsKey("Capacity"));
    }

    // ---------- Price ----------

    [Fact]
    public void Missing_price_is_rejected()
    {
        var request = ValidRequest() with { Price = null };

        var errors = EventEndpoints.Validate(request);

        Assert.Contains("Price", errors.Keys);
        Assert.Single(errors);
    }

    // decimal values can't be written inside [InlineData], so these rows live here instead.
    public static TheoryData<decimal, bool> Prices => new()
    {
        { -0.01m, false },
        { 0m, true },
        { 25.50m, true },
        { 10.999m, false },
        { 9_999_999_999.99m, true },
        { 10_000_000_000m, false },
    };

    [Theory]
    [MemberData(nameof(Prices))]
    public void Price_must_be_0_or_more_with_at_most_2_decimals(decimal price, bool expectedValid)
    {
        var request = ValidRequest() with { Price = price };

        var errors = EventEndpoints.Validate(request);

        Assert.Equal(expectedValid, !errors.ContainsKey("Price"));
    }

    // ---------- MapToEvent ----------

    [Fact]
    public void Blank_description_is_stored_as_null()
    {
        var request = ValidRequest() with { Description = "   " };

        var mapped = EventEndpoints.MapToEvent(request);

        Assert.Null(mapped.Description);
    }

    [Fact]
    public void Mapping_copies_the_request_and_trims_text()
    {
        var request = ValidRequest() with
        {
            Name = "  Summer Concert  ",
            Description = "  Live music  ",
            Venue = "  City Hall  ",
        };

        var mapped = EventEndpoints.MapToEvent(request);

        Assert.Equal("Summer Concert", mapped.Name);
        Assert.Equal("Live music", mapped.Description);
        Assert.Equal("City Hall", mapped.Venue);
        Assert.Equal(request.StartsAtUtc, mapped.StartsAtUtc);
        Assert.Equal(request.Capacity, mapped.Capacity);
        Assert.Equal(request.Price, mapped.Price);
    }

    [Fact]
    public void Mapping_sets_the_fields_the_client_must_not_control()
    {
        var before = DateTime.UtcNow;

        var mapped = EventEndpoints.MapToEvent(ValidRequest());

        Assert.NotEqual(Guid.Empty, mapped.Id);
        Assert.Equal(0, mapped.SeatsTaken);
        Assert.Equal(mapped.CreatedAtUtc, mapped.UpdatedAtUtc);
        Assert.InRange(mapped.CreatedAtUtc, before, DateTime.UtcNow);
    }

    [Fact]
    public void Mapping_gives_each_event_a_new_id()
    {
        var first = EventEndpoints.MapToEvent(ValidRequest());
        var second = EventEndpoints.MapToEvent(ValidRequest());

        Assert.NotEqual(first.Id, second.Id);
    }
}
