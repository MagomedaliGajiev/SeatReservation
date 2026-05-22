using SeatReservation.Domain.Events;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Domain.Reservations;

public record ReservationId(Guid Value);

public class Reservation
{
    private List<ReservationSeat> _reservedSeats;

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
            .Select(seatId => new ReservationSeat(new ReservationSeatId(Guid.NewGuid()), this, new SeatId(seatId)))
            .ToList();

        _reservedSeats = reservedSeats;
    }

    public ReservationId Id { get; }

    public EventId EventId { get; private set; }

    public Guid UserId { get; private set; }

    public ReservationStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<ReservationSeat> ReservedSeats => _reservedSeats;
}