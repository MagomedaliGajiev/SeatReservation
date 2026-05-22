using Microsoft.EntityFrameworkCore;
using SeatReservation.Application;
using SeatReservation.Application.Database;
using SeatReservation.Infrastructure.Postgres;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddDbContext<ReservationServiceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ReservationServiceDb")));

builder.Services.AddScoped<IReservationDbContext>(sp =>
    sp.GetRequiredService<ReservationServiceDbContext>());

builder.Services.AddScoped<CreateVenueHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "SeatReservationService"));
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();