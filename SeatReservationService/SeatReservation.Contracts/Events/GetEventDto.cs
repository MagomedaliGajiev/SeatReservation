using SeatReservation.Contracts.Seats;

namespace SeatReservation.Contracts.Events;

public record GetEventDto
{
    public Guid Id { get; init;  }

    public int Capacity { get; init; }

    public string Description { get; init; } = string.Empty;

    public DateTime? LastReservationUtc { get; init; }

    public Guid VenueId { get; init; }

    public string Name { get; init; } = null!;

    public string Type { get; init; } = string.Empty;

    public DateTime EventDate { get; init; }

    public DateTime StartDate { get; init; }

    public DateTime EndDate { get; init; }

    public string Status { get; init; } = string.Empty;

    public string Info { get; init; } = string.Empty;

    public List<AvailableSeatsDto> Seats { get; init; } = [];
}