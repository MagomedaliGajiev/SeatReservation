using Microsoft.EntityFrameworkCore;
using SeatReservation.Application;
using SeatReservation.Application.Database;
using SeatReservation.Infrastructure.Postgres;
using SeatReservation.Infrastructure.Postgres.Database;
using SeatReservation.Infrastructure.Postgres.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddDbContext<ReservationServiceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ReservationServiceDb")));

builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();

builder.Services.AddScoped<IVenuesRepository, NpgSqlVenuesRepository>();
// builder.Services.AddScoped<IVenuesRepository, EfCoreVenuesRepository>();

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