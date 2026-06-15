using CSharpFunctionalExtensions;
using SeatReservation.Application.Database;
using SeatReservation.Application.Events;
using SeatReservation.Application.Seats;
using SeatReservation.Application.Venues;
using SeatReservation.Contracts;
using SeatReservation.Contracts.Reservation;
using SeatReservation.Contracts.Seats;
using SeatReservation.Domain.Events;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;
using SharedKernel;

namespace SeatReservation.Application.Reservations;

public class ReserveAdjacentSeatsHandler
{
    private const int MaxAdjacentSeats = 10;

    private readonly ISeatsRepository _seatsRepository;
    private readonly IReservationsRepository _reservationsRepository;
    private readonly IEventsRepository _eventsRepository;
    private readonly ITransactionManager _transactionManager;

    public ReserveAdjacentSeatsHandler(
        ISeatsRepository seatsRepository,
        IReservationsRepository reservationsRepository,
        IEventsRepository eventsRepository,
        ITransactionManager transactionManager)
    {
        _seatsRepository = seatsRepository;
        _reservationsRepository = reservationsRepository;
        _transactionManager = transactionManager;
        _eventsRepository = eventsRepository;
    }

    public async Task<Result<Guid, Error>> Handle(ReserveAdjacentSeatsRequest request,
        CancellationToken cancellationToken)
    {
        if (request.RequiredSeatsCount <= 0)
        {
            return Error.Validation("reserveAdjacent.seatsCount", "Required seats count must be greater than zero");
        }

        if (request.RequiredSeatsCount > MaxAdjacentSeats)
        {
            return Error.Validation(
                "reserveAdjacent.seatsCount",
                $"Cannot reserve more than {MaxAdjacentSeats} adjacent seats at once");
        }

        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionResult.IsFailure)
        {
            return transactionResult.Error;
        }

        using var transaction = transactionResult.Value;

        (_, bool isFailure, Event? @event, Error? error) = await _eventsRepository.GetByIdWithLock(new EventId(request.EventId), cancellationToken);

        if (isFailure)
        {
            return error;
        }

        // Площадка в запросе должна совпадать с площадкой мероприятия
        if (@event.VenueId != new VenueId(request.VenueId))
        {
            return Error.Conflict("reserveAdjacent.venue", "Venue does not belong to event");
        }

        // Доступно ли мероприятие для бронирования (статус, даты, вместимость)
        int reservedSeatsCount = await _reservationsRepository.GetReservedSeatsCount(
            new EventId(request.EventId),
            cancellationToken);

        if (!@event.IsAvailableForReservation(reservedSeatsCount + request.RequiredSeatsCount))
        {
            return Error.Failure("reservation.unavailable", "Reservation is unavailable");
        }

        var availableSeats = await _seatsRepository.GetAvailableSeats(
            @event.VenueId,
            new EventId(request.EventId),
            request.PreferredRowNumber,
            cancellationToken);

        if (availableSeats.Count == 0)
        {
            return Error.NotFound("reserveAdjacent.seatsCount", "No available seats found");
        }

        var selectedSeats = request.PreferredRowNumber.HasValue ?
            AdjacentSeatsFinder.FindAdjacentSeatsInPreferredRow(availableSeats, request.RequiredSeatsCount, request.PreferredRowNumber.Value)
            : AdjacentSeatsFinder.FindBestAdjacentSeats(availableSeats, request.RequiredSeatsCount);

        if (selectedSeats.Count == 0)
        {
            return Error.NotFound(
                "reserveAdjacent.seatsCount",
                $"Could not find {request.RequiredSeatsCount} adjacent available seats");
        }

        var seatIds = selectedSeats.Select(s => s.Id).ToList();

        var reservationResult = Reservation.Create(
            new EventId(request.EventId),
            request.UserId,
            seatIds.Select(id => id.Value));

        if (reservationResult.IsFailure)
        {
            return reservationResult.Error;
        }

        var reservation = reservationResult.Value;

        var addResult = await _reservationsRepository.Add(reservation, cancellationToken);

        if (addResult.IsFailure)
        {
            return addResult.Error;
        }

        @event.Details.ReserveSeat();

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Error;
        }

        var commitResult = transaction.Commit();
        if (commitResult.IsFailure)
        {
            return commitResult.Error;
        }

        return addResult.Value;
    }
}
