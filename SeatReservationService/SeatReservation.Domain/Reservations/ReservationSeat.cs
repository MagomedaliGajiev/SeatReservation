using SeatReservation.Domain.Events;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Domain.Reservations;

public record ReservationSeatId(Guid Value);

public class ReservationSeat
{
    // EF Core
    private ReservationSeat()
    {
    }

    public ReservationSeat(ReservationSeatId id, Reservation reservation, SeatId seatId, EventId eventId)
    {
        Id = id;
        Reservation = reservation;
        SeatId = seatId;
        EventId = eventId;
        ReservedAt = DateTime.UtcNow;
    }

    public ReservationSeatId Id { get; }

    public ReservationId ReservationId { get; private set; }

    public Reservation Reservation { get; private set; }

    public SeatId SeatId { get; private set; }

    public EventId EventId { get; private set; }

    public DateTime ReservedAt { get; }
}