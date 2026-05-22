using Microsoft.EntityFrameworkCore;
using SeatReservation.Domain;
using SeatReservation.Domain.Events;
using SeatReservation.Domain.Venues;
using SeatReservation.Infrastructure.Postgres;
using EventId = SeatReservation.Domain.Events.EventId;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddScoped<ReservationServiceDbContext>(_ =>
    new ReservationServiceDbContext(builder.Configuration.GetConnectionString("ReservationServiceDb")!));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "SeatReservationService"));
}

app.UseHttpsRedirection();

app.MapPost("/users", (ReservationServiceDbContext dbContext) =>
{
    var social = new SocialNetwork() { Link = "Test", Name = "Test", };

    dbContext.Add(new User()
    {
        Details = new Details()
        {
            Description = "Test",
            FIO = "Test",
            Socials = [social],
        },
    });

    dbContext.SaveChangesAsync();

    return Results.Ok();
});

app.MapGet("/users", (ReservationServiceDbContext dbContext) =>
{
    return dbContext.Users.Where(u => u.Details.Socials.Any(s => s.Link == "Test")).ToListAsync();
});

app.MapPost("/event", (ReservationServiceDbContext dbContext) =>
{
    var venueId = new VenueId(Guid.Parse("8bb07e4a-0e98-4040-bc69-f70e7f03171d"));
    var venueName = VenueName.Create("test", "test").Value;

    dbContext.Add(new Venue(venueId, venueName, 100, []));

    dbContext.Add(
        new Event(
            new EventId(Guid.NewGuid()),
            new EventDetails(10, "test"),
            venueId,
            "test",
            DateTime.UtcNow,
            EventType.CONFERENCE,
            new ConferenceInfo("Kirill", "EfCore")));

    dbContext.Add(
        new Event(
            new EventId(Guid.NewGuid()),
            new EventDetails(10, "test"),
            venueId,
            "test",
            DateTime.UtcNow,
            EventType.ONLINE,
            new OnlineInfo("url")));

    dbContext.Add(
        new Event(
            new EventId(Guid.NewGuid()),
            new EventDetails(10, "test"),
            venueId,
            "test",
            DateTime.UtcNow,
            EventType.CONCERT,
            new ConcertInfo("Test")));

    dbContext.SaveChanges();
});

app.Run();