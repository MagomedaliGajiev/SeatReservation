using Microsoft.EntityFrameworkCore;
using SeatReservation.Domain;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Infrastructure.Postgres;

public class ReservationServiceDbContext : DbContext
{
    private readonly string _connectionString;

    public ReservationServiceDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReservationServiceDbContext).Assembly);
    }

    public DbSet<Venue> Venues => Set<Venue>();

    public DbSet<User> Users => Set<User>();
}