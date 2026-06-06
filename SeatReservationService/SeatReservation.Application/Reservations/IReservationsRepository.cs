using CSharpFunctionalExtensions;
using SeatReservation.Domain.Events;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;
using SharedKernel;

namespace SeatReservation.Application.Reservations;

public interface IReservationsRepository
{
    Task<Result<Guid, Error>> Add(Reservation reservation, CancellationToken cancellationToken);

    Task<bool> AnySeatsAlreadyReserved(EventId eventId, IEnumerable<SeatId> seatIds, CancellationToken cancellationToken);
}