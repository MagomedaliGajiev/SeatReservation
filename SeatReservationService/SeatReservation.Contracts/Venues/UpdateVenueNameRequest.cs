namespace SeatReservation.Contracts.Venues;

public record UpdateVenueNameRequest(Guid Id, string Name);