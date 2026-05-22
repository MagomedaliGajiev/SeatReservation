namespace SeatReservation.Domain.Events;

public class EventDetails
{
    // EF Core
    private EventDetails()
    {
    }

    public EventDetails(int capacity, string description)
    {
        Capacity = capacity;
        Description = description;
    }

    public EventId EventId { get; }

    public int Capacity { get; private set; }

    public string Description { get; private set; }
}