using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SeatReservation.Application.Database;
using SeatReservation.Application.Events;
using SeatReservation.Application.Events.Commands;
using SeatReservation.Application.Events.Queries;
using SeatReservation.Application.Reservations;
using SeatReservation.Application.Reservations.Commands;
using SeatReservation.Application.Seats;
using SeatReservation.Application.Venues;
using SeatReservation.Application.Venues.Commands;
using SeatReservation.Infrastructure.Postgres;
using SeatReservation.Infrastructure.Postgres.Database;
using SeatReservation.Infrastructure.Postgres.Repositories;
using SeatReservation.Infrastructure.Postgres.Seeding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddDbContext<ReservationServiceDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("ReservationServiceDb"));

    // Диагностика, раскрывающая значения параметров SQL (потенциально персональные
    // данные) и детальные ошибки, — только для локальной разработки, не для прода.
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

// Тот же scoped-экземпляр контекста отдаём и под интерфейсом чтения для CQRS-запросов.
builder.Services.AddScoped<IReadDbContext>(sp =>
    sp.GetRequiredService<ReservationServiceDbContext>());

builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
builder.Services.AddScoped<ITransactionManager, TransactionManager>();

// builder.Services.AddScoped<IVenuesRepository, NpgSqlVenuesRepository>();
builder.Services.AddScoped<IVenuesRepository, VenuesRepository>();
builder.Services.AddScoped<IEventsRepository, EventsRepository>();
builder.Services.AddScoped<IReservationsRepository, ReservationsRepository>();
builder.Services.AddScoped<ISeatsRepository, SeatsRepository>();

builder.Services.AddValidatorsFromAssembly(typeof(ReserveRequestValidator).Assembly);

builder.Services.AddScoped<CreateVenueHandler>();
builder.Services.AddScoped<UpdateVenueNameHandler>();
builder.Services.AddScoped<UpdateVenueNameByPrefixHandler>();
builder.Services.AddScoped<UpdateVenueSeatsHandler>();
builder.Services.AddScoped<CreateEventHandler>();
builder.Services.AddScoped<ReserveHandler>();
builder.Services.AddScoped<ReserveAdjacentSeatsHandler>();
builder.Services.AddScoped<GetEventByIdHandler>();

builder.Services.AddScoped<ISeeder, ReservationSeeder>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "SeatReservationService"));

    if (args.Contains("--seeding"))
    {
        await app.Services.RunSeeding();
    }
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();