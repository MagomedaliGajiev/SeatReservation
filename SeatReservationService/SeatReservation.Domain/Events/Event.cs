namespace SeatReservation.Domain.Events;

public class Event
{
    public Event(Guid id, EventDetails eventDetails, Guid venueId, string name, DateTime eventDate)
    {
        Id = id;
        EventDetails = eventDetails;
        VenueId = venueId;
        Name = name;
        EventDate = eventDate;
    }

    public Guid Id { get; }

    public EventDetails EventDetails { get; private set; }

    public Guid VenueId { get; private set; }

    public string Name { get; private set; }

    public DateTime EventDate { get; private set; }
}
