namespace SeatReservation.Contracts.Seats;

public record AvailableSeatsDto
{
    public Guid Id { get; init; }

    public int RowNumber { get; init; }

    public int SeatNumber { get; init; }

    public Guid VenueId { get; init; }

    public bool IsAvailable { get; init; }
}