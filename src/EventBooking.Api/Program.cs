using EventBooking.Modules.Bookings;
using EventBooking.Modules.Events;
using EventBooking.Modules.Notifications;
using EventBooking.Modules.Payments;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .AddEventsModule()
    .AddBookingsModule()
    .AddPaymentsModule()
    .AddNotificationsModule();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

public partial class Program;
