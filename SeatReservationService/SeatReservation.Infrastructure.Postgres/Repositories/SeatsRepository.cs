using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeatReservation.Application.Seats;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Infrastructure.Postgres.Repositories;

public class SeatsRepository : ISeatsRepository
{
    private readonly ReservationServiceDbContext _dbContext;
    private readonly ILogger<SeatsRepository> _logger;

    public SeatsRepository(ReservationServiceDbContext dbContext, ILogger<SeatsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Seat>> GetByIds(IEnumerable<SeatId> seatIds, CancellationToken cancellationToken)
    {
        return await _dbContext.Seats.Where(s => seatIds.Contains(s.Id)).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Seat>> GetAvailableSeats(
        VenueId venueId,
        Domain.Events.EventId eventId,
        int? rowNumber,
        CancellationToken cancellationToken)
    {
        var seats = await _dbContext.Seats
            .Where(s => s.VenueId == venueId)
            .Where(s => !rowNumber.HasValue || s.RowNumber == rowNumber.Value)
            .Where(s => !_dbContext.ReservationSeats.Any(rs =>
                rs.SeatId == s.Id &&
                rs.EventId == eventId &&
                (rs.Reservation.Status == ReservationStatus.CONFIRMED || rs.Reservation.Status == ReservationStatus.PENDING)))
            .ToListAsync(cancellationToken);

        return seats;
    }
}
