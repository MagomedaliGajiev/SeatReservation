using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeatReservation.Domain;
using SeatReservation.Domain.Events;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Infrastructure.Postgres;

public class ReservationServiceDbContext : DbContext
{
    public ReservationServiceDbContext(DbContextOptions<ReservationServiceDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseLoggerFactory(CreateLoggerFactory());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReservationServiceDbContext).Assembly);
    }

    public DbSet<Venue> Venues => Set<Venue>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Seat> Seats => Set<Seat>();

    public DbSet<Reservation> Reservations => Set<Reservation>();

    public DbSet<ReservationSeat> ReservationSeats => Set<ReservationSeat>();

    public DbSet<Event> Events => Set<Event>();

    private ILoggerFactory CreateLoggerFactory() =>
        LoggerFactory.Create(builder => { builder.AddConsole(); });
}