using Core.Validation;
using CSharpFunctionalExtensions;
using FluentValidation;
using SeatReservation.Application.Events;
using SeatReservation.Application.Seats;
using SeatReservation.Contracts;
using SeatReservation.Domain.Events;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;
using SharedKernel;

namespace SeatReservation.Application.Reservations;

public class ReserveHandler
{
    private readonly IReservationsRepository _reservationsRepository;
    private readonly IEventsRepository _eventsRepository;
    private readonly ISeatsRepository _seatsRepository;
    private readonly IValidator<ReserveRequest> _validator;

    public ReserveHandler(
        IReservationsRepository reservationsRepository,
        IEventsRepository eventsRepository,
        ISeatsRepository seatsRepository,
        IValidator<ReserveRequest> validator)
    {
        _reservationsRepository = reservationsRepository;
        _eventsRepository = eventsRepository;
        _seatsRepository = seatsRepository;
        _validator = validator;
    }

    public async Task<Result<Guid, Error>> Handle(ReserveRequest request, CancellationToken cancellationToken)
    {
        // Бронирование мест на мероприятии

        // 1. Валидация входных параметров
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        // 2. Доступно ли это мероприятие для бронирования. Проверить даты. Проверить статус.
        var eventId = new EventId(request.EventId);
        (_, bool isFailure, Event? @event, Error? error) = await _eventsRepository.GetById(eventId, cancellationToken);
        if (isFailure)
        {
            return error;
        }

        if (!@event.IsAvailableForReservation())
        {
            return Error.Failure("reservation.unavailable", "Reservation is unavailable");
        }

        // 3. Проверить, что места принадлежат мероприятию или площадке.
        var seatIds = request.SeatIds.Select(id => new SeatId(id)).ToList();
        var seats = await _seatsRepository.GetByIds(seatIds, cancellationToken);

        if (seats.Any(seat => seat.VenueId != @event.VenueId) || seats.Count == 0)
        {
            return Error.Conflict("seats.conflict", "Seat does not belong to venue");
        }

        // 4. Проверить, что места не забронированы на нужное мероприятие.
        var isSeatsReserved = await _reservationsRepository.AnySeatsAlreadyReserved(new EventId(request.EventId), seatIds, cancellationToken);
        if (isSeatsReserved)
        {
            return Error.Conflict("seats.conflict", "Seats already reserved");
        }

        // Создать Reservation с ReservedSeats
        var reservationResult = Reservation.Create(new EventId(request.EventId), request.UserId, request.SeatIds);
        if (reservationResult.IsFailure)
        {
            return reservationResult.Error;
        }

        // Сохранить бронирование в базу данных
        await _reservationsRepository.Add(reservationResult.Value, cancellationToken);

        return reservationResult.Value.Id.Value;
    }
}