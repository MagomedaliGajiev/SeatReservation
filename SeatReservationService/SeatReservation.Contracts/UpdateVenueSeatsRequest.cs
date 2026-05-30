namespace SeatReservation.Contracts;

public record UpdateVenueSeatsRequest(Guid VenueId, IEnumerable<UpdateSeatRequest> Seats);