using SeatReservation.Contracts.Seats;

namespace SeatReservation.Contracts.Venues;

public record CreateVenueRequest(string Prefix, string Name, int SeatsLimit, IEnumerable<CreateSeatRequest> Seats);