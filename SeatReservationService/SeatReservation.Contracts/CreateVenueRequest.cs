namespace SeatReservation.Contracts;

public record CreateVenueRequest(string Prefix, string Name, int SeatsLimit, IEnumerable<CreateSeatRequest> Seats);