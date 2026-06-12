using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeatReservation.Application.Reservations;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;
using SharedKernel;
using EventId = SeatReservation.Domain.Events.EventId;

namespace SeatReservation.Infrastructure.Postgres.Repositories;

public class ReservationsRepository : IReservationsRepository
{
    private readonly ReservationServiceDbContext _dbContext;
    private readonly ILogger<ReservationsRepository> _logger;

    public ReservationsRepository(ReservationServiceDbContext dbContext, ILogger<ReservationsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Add(Reservation reservation, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Reservations.AddAsync(reservation, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return reservation.Id.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fail to insert reservation");

            return Error.Failure("venue.insert", "Fail to insert reservation");
        }
    }

    public async Task<bool> AnySeatsAlreadyReserved(
        EventId eventId,
        IEnumerable<SeatId> seatIds,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Reservations
            .Where(r => r.EventId == eventId)
            .Where(r => r.ReservedSeats.Any(rs => seatIds.Contains(rs.SeatId)))
            .SelectMany(r => r.ReservedSeats)
            .AnyAsync(cancellationToken);
    }

    public async Task<int> GetReservedSeatsCount(EventId eventId, CancellationToken cancellationToken)
    {
        // await _dbContext.Database.ExecuteSqlAsync(
        //     $"SELECT capacity FROM events_details WHERE event_id = {eventId} FOR UPDATE", cancellationToken));

        return await _dbContext.Reservations
            .Where(r => r.EventId == eventId)
            .Where(r => r.Status == ReservationStatus.CONFIRMED
                        || r.Status == ReservationStatus.PENDING)
            .SelectMany(r => r.ReservedSeats)
            .CountAsync(cancellationToken);
    }
}