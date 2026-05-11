namespace SeatReservation.Domain.Reservations;

public class ReservationSeat
{
    public ReservationSeat(Guid id, Reservation reservationId, Guid seatId)
    {
        Id = id;
        ReservationId = reservationId;
        SeatId = seatId;
        ReservedAt = DateTime.UtcNow;
    }

    public Guid Id { get; }

    public Reservation ReservationId { get; private set; }

    public Guid SeatId { get; private set; }

    public DateTime ReservedAt { get; }
}
