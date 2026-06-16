using SeatReservation.Contracts.Seats;

namespace SeatReservation.Contracts.Venues;

public record UpdateVenueSeatsRequest(Guid VenueId, IEnumerable<UpdateSeatRequest> Seats);