namespace SeatReservation.Contracts.Reservation;

public record ReserveRequest(Guid EventId, Guid UserId, IEnumerable<Guid> SeatIds);