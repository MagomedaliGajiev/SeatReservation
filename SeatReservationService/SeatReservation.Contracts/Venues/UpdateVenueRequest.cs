namespace SeatReservation.Contracts.Venues;

public record UpdateVenueRequest(Guid VenueId, string Prefix, string Name, int SeatsLimit);