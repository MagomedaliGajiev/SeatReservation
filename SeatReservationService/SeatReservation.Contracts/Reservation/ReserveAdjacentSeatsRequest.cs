namespace SeatReservation.Contracts.Reservation;

public record ReserveAdjacentSeatsRequest(
    Guid EventId,
    Guid UserId,
    Guid VenueId,
    int RequiredSeatsCount,
    int? PreferredRowNumber);
