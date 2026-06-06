using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SeatReservation.Application.Database;
using SeatReservation.Application.Reservations;
using SeatReservation.Application.Venues;
using SeatReservation.Infrastructure.Postgres;
using SeatReservation.Infrastructure.Postgres.Database;
using SeatReservation.Infrastructure.Postgres.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddDbContext<ReservationServiceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ReservationServiceDb")));

builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
builder.Services.AddScoped<ITransactionManager, TransactionManager>();

// builder.Services.AddScoped<IVenuesRepository, NpgSqlVenuesRepository>();
builder.Services.AddScoped<IVenuesRepository, VenuesRepository>();

builder.Services.AddValidatorsFromAssembly(typeof(ReserveRequestValidator).Assembly);

builder.Services.AddScoped<CreateVenueHandler>();
builder.Services.AddScoped<UpdateVenueNameHandler>();
builder.Services.AddScoped<UpdateVenueNameByPrefixHandler>();
builder.Services.AddScoped<UpdateVenueSeatsHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "SeatReservationService"));
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();