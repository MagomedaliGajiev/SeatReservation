using Microsoft.EntityFrameworkCore;
using SeatReservation.Application.Database;
using SeatReservation.Domain;
using SeatReservation.Domain.Events;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Infrastructure.Postgres;

public class ReservationServiceDbContext : DbContext, IReadDbContext
{
    public ReservationServiceDbContext(DbContextOptions<ReservationServiceDbContext> options)
        : base(options)
    {
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

    public IQueryable<Event> EventsRead => Set<Event>().AsQueryable().AsNoTracking();
}