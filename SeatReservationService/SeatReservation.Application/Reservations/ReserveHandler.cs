using System.Data;
using Core.Validation;
using CSharpFunctionalExtensions;
using FluentValidation;
using SeatReservation.Application.Database;
using SeatReservation.Application.Events;
using SeatReservation.Application.Seats;
using SeatReservation.Contracts;
using SeatReservation.Contracts.Reservation;
using SeatReservation.Contracts.Seats;
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
    private readonly ITransactionManager _transactionManager;

    public ReserveHandler(
        IReservationsRepository reservationsRepository,
        IEventsRepository eventsRepository,
        ISeatsRepository seatsRepository,
        IValidator<ReserveRequest> validator,
        ITransactionManager transactionManager)
    {
        _reservationsRepository = reservationsRepository;
        _eventsRepository = eventsRepository;
        _seatsRepository = seatsRepository;
        _validator = validator;
        _transactionManager = transactionManager;
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

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }

        using var transactionScope = transactionScopeResult.Value;

        // 2. Доступно ли это мероприятие для бронирования. Проверить даты. Проверить статус.
        var eventId = new EventId(request.EventId);
        (_, bool isFailure, Event? @event, Error? error) = await _eventsRepository.GetByIdWithLock(eventId, cancellationToken);
        if (isFailure)
        {
            return error;
        }

        int reservedSeatsCount = await _reservationsRepository.GetReservedSeatsCount(eventId, cancellationToken);

        if (!@event.IsAvailableForReservation(reservedSeatsCount + request.SeatIds.Count()))
        {
            transactionScope.Rollback();
            return Error.Failure("reservation.unavailable", "Reservation is unavailable");
        }

        // 3. Проверить, что места принадлежат мероприятию или площадке.
        var seatIds = request.SeatIds.Select(id => new SeatId(id)).ToList();
        var seats = await _seatsRepository.GetByIds(seatIds, cancellationToken);

        if (seats.Any(seat => seat.VenueId != @event.VenueId) || seats.Count == 0)
        {
            transactionScope.Rollback();
            return Error.Conflict("seats.conflict", "Seat does not belong to venue");
        }

        // 4. Проверить, что места не забронированы на нужное мероприятие.
        // var isSeatsReserved = await _reservationsRepository.AnySeatsAlreadyReserved(new EventId(request.EventId), seatIds, cancellationToken);
        // if (isSeatsReserved)
        // {
        //     return Error.Conflict("seats.conflict", "Seats already reserved");
        // }

        // Создать Reservation с ReservedSeats
        var reservationResult = Reservation.Create(new EventId(request.EventId), request.UserId, request.SeatIds);
        if (reservationResult.IsFailure)
        {
            return reservationResult.Error;
        }

        // Сохранить бронирование в базу данных
        var insertResult = await _reservationsRepository.Add(reservationResult.Value, cancellationToken);
        if (insertResult.IsFailure)
        {
            transactionScope.Rollback();
            return insertResult.Error;
        }

        @event.Details.ReserveSeat();

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveResult.Error;
        }

        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
        {
            transactionScope.Rollback();
            return commitedResult.Error;
        }

        return reservationResult.Value.Id.Value;
    }
}