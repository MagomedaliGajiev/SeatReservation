namespace SeatReservation.Contracts.Seats;

public record SeatsDto
{
    public Guid Id { get; init; }

    public int RowNumber { get; init; }

    public int SeatNumber { get; init; }

    public Guid VenueId { get; init; }
}