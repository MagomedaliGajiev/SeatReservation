namespace SeatReservation.Domain.Reservations;

public class Reservation
{
    private List<ReservationSeat> _reservedSeats;

    public Reservation(Guid id, Guid eventId, Guid userId, IEnumerable<Guid> seatIds)
    {
        Id = id;
        EventId = eventId;
        UserId = userId;
        Status = ReservationStatus.PENDING;
        CreatedAt = DateTime.UtcNow;

        var reservedSeats = seatIds
            .Select(seatId => new ReservationSeat(Guid.NewGuid(), this, seatId))
            .ToList();

        _reservedSeats = reservedSeats;
    }

    public Guid Id { get; }

    public Guid EventId { get; private set; }

    public Guid UserId { get; private set; }

    public ReservationStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<ReservationSeat> ReservedSeats => _reservedSeats;
}