namespace SeatReservation.Contracts;

public record UpdateVenueRequest(Guid VenueId, string Prefix, string Name, int SeatsLimit);