using SeatReservation.Domain.Venues;

namespace SeatReservation.Domain.Events;

public record EventId(Guid Value);

public class Event
{
    // EF Core
    private Event(EventType type, IEventInfo info)
    {
        Type = type;
        Info = info;
    }

    public Event(
        EventId id,
        EventDetails eventDetails,
        VenueId venueId,
        string name,
        DateTime eventDate,
        EventType type,
        IEventInfo info)
    {
        Id = id;
        EventDetails = eventDetails;
        VenueId = venueId;
        Name = name;
        EventDate = eventDate;
        Type = type;
        Info = info;
    }

    public EventId Id { get; }

    public EventDetails EventDetails { get; private set; }

    public VenueId VenueId { get; private set; }

    public string Name { get; private set; }

    public EventType Type { get; private set; }

    public DateTime EventDate { get; private set; }

    public IEventInfo Info { get; private set; }
}

public enum EventType
{
    CONCERT,
    CONFERENCE,
    ONLINE
}

public interface IEventInfo
{
    string ToString();
}

public record ConcertInfo(string Performer) : IEventInfo
{
    public override string ToString() => $"Concert:{Performer}";
}

public record ConferenceInfo(string Speaker, string Topic) : IEventInfo
{
    public override string ToString() => $"Conference:{Speaker}|{Topic}";
}

public record OnlineInfo(string Url) : IEventInfo
{
    public override string ToString() => $"Online:{Url}";
}