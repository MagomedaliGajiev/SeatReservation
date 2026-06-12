using CSharpFunctionalExtensions;
using SeatReservation.Domain.Events;
using SeatReservation.Domain.Venues;
using SharedKernel;

namespace SeatReservation.Domain.Reservations;

public record ReservationId(Guid Value);

public class Reservation
{
    private List<ReservationSeat> _reservedSeats = [];

    // EF Core
    private Reservation()
    {
    }

    public Reservation(ReservationId id, EventId eventId, Guid userId, IEnumerable<Guid> seatIds)
    {
        Id = id;
        EventId = eventId;
        UserId = userId;
        Status = ReservationStatus.PENDING;
        CreatedAt = DateTime.UtcNow;

        var reservedSeats = seatIds
            .Select(seatId => new ReservationSeat(new ReservationSeatId(Guid.NewGuid()), this, new SeatId(seatId), eventId))
            .ToList();

        _reservedSeats = reservedSeats;
    }

    public ReservationId Id { get; }

    public EventId EventId { get; private set; }

    public Guid UserId { get; private set; }

    public ReservationStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<ReservationSeat> ReservedSeats => _reservedSeats;

    public static Result<Reservation, Error> Create(
        EventId eventId,
        Guid userId,
        IEnumerable<Guid> seatIds)
    {
        if (eventId.Value == Guid.Empty)
        {
            return Error.Validation("reservation.eventId", "Event ID cannot be empty");
        }

        if (userId == Guid.Empty)
        {
            return Error.Validation("reservation.userId", "User ID cannot be empty");
        }

        var seatIdsList = seatIds?.ToList() ?? [];

        if (seatIdsList.Count == 0)
        {
            return Error.Validation("reservation.seats", "At least one seat must be selected");
        }

        if (seatIdsList.Any(seatId => seatId == Guid.Empty))
        {
            return Error.Validation("reservation.seats", "Seat IDs cannot be empty");
        }

        return new Reservation(new ReservationId(Guid.NewGuid()), eventId, userId, seatIdsList);
    }
}