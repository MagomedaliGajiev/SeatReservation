namespace SeatReservation.Contracts;

public record CreateEventRequest(
    Guid VenueId,
    string Name,
    DateTime EventDate,
    DateTime StartDate,
    DateTime EndDate,
    int Capacity,
    string Description,
    EventTypeDto Type,
    string? Performer,
    string? Speaker,
    string? Topic,
    string? Url);

public enum EventTypeDto
{
    CONCERT,
    CONFERENCE,
    ONLINE
}