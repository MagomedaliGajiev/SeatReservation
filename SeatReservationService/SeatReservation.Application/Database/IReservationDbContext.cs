using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Application.Database;

public interface IReservationDbContext
{
    DbSet<Venue> Venues { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    ChangeTracker ChangeTracker { get; }
}