using CSharpFunctionalExtensions;
using SeatReservation.Domain.Venues;
using SharedKernel;

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
        VenueId venueId,
        EventDetails details,
        string name,
        DateTime eventDate,
        DateTime startDate,
        DateTime endDate,
        IEventInfo eventInfo,
        EventType type)
    {
        Id = id;
        VenueId = venueId;
        Name = name;
        EventDate = eventDate;
        Info = eventInfo;
        Details = details;
        Type = type;
        StartDate = startDate;
        EndDate = endDate;
        Status = EventStatus.PLANNED;
    }

    public EventId Id { get; } = null!;

    public EventDetails Details { get; private set; } = null!;

    public VenueId VenueId { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public EventType Type { get; private set; }

    public DateTime EventDate { get; private set; }

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public EventStatus Status { get; private set; }

    public IEventInfo Info { get; private set; } = null!;

    public bool IsAvailableForReservation(int capacitySum)
        => Status == EventStatus.PLANNED && StartDate > DateTime.UtcNow && capacitySum <= Details.Capacity;

    private static Result<EventDetails, Error> Validate(
        string name,
        DateTime eventDate,
        DateTime startDate,
        DateTime endDate,
        int capacity,
        string description)
    {
        if (startDate >= endDate || startDate <= DateTime.UtcNow || endDate <= DateTime.UtcNow)
        {
            return Error.Validation("event.time", "The time of the event is indicated incorrectly");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("event.name", "Event name cannot be empty");
        }

        if (eventDate < DateTime.UtcNow)
        {
            return Error.Validation("event.date", "Event date cannot be in the past");
        }

        if (capacity <= 0)
        {
            return Error.Validation("event.capacity", "Capacity must be greater than zero");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Error.Validation("event.description", "Description cannot be empty");
        }

        return new EventDetails(capacity, description);
    }

    public static Result<Event, Error> CreateConcert(
        VenueId venueId,
        string name,
        DateTime eventDate,
        DateTime startDate,
        DateTime endDate,
        int capacity,
        string description,
        string performer)
    {
        var detailsResult = Validate(name, eventDate, startDate, endDate, capacity, description);
        if (detailsResult.IsFailure)
        {
            return detailsResult.Error;
        }

        if (string.IsNullOrWhiteSpace(performer))
        {
            return Error.Validation("event.performer", "Performer cannot be empty");
        }

        var concertInfo = new ConcertInfo(performer);

        return new Event(
            new EventId(Guid.NewGuid()),
            venueId,
            detailsResult.Value,
            name,
            eventDate,
            startDate,
            endDate,
            concertInfo,
            EventType.CONCERT);
    }

    public static Result<Event, Error> CreateConference(
        VenueId venueId,
        string name,
        DateTime eventDate,
        DateTime startDate,
        DateTime endDate,
        int capacity,
        string description,
        string speaker,
        string topic)
    {
        var detailsResult = Validate(name, eventDate, startDate, endDate, capacity, description);
        if (detailsResult.IsFailure)
        {
            return detailsResult.Error;
        }

        if (string.IsNullOrWhiteSpace(speaker))
        {
            return Error.Validation("event.speaker", "Speaker cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(topic))
        {
            return Error.Validation("event.topic", "Topic cannot be empty");
        }

        var conferenceInfo = new ConferenceInfo(speaker, topic);

        return new Event(
            new EventId(Guid.NewGuid()),
            venueId,
            detailsResult.Value,
            name,
            eventDate,
            startDate,
            endDate,
            conferenceInfo,
            EventType.CONFERENCE);
    }

    public static Result<Event, Error> CreateOnline(
        VenueId venueId,
        string name,
        DateTime eventDate,
        DateTime startDate,
        DateTime endDate,
        int capacity,
        string description,
        string url)
    {
        var detailsResult = Validate(name, eventDate, startDate, endDate, capacity, description);
        if (detailsResult.IsFailure)
        {
            return detailsResult.Error;
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            return Error.Validation("event.url", "URL cannot be empty");
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return Error.Validation("event.url", "URL is not valid");
        }

        var onlineInfo = new OnlineInfo(url);

        return new Event(
            new EventId(Guid.NewGuid()),
            venueId,
            detailsResult.Value,
            name,
            eventDate,
            startDate,
            endDate,
            onlineInfo,
            EventType.ONLINE);
    }
}

public enum EventStatus
{
    PLANNED,
    IN_PROGRESS,
    FINISHED,
    CANCELLED,
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