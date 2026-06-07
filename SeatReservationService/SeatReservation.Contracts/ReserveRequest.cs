namespace SeatReservation.Contracts;

public record ReserveRequest(Guid EventId, Guid UserId, IEnumerable<Guid> SeatIds);