namespace SeatReservation.Domain.Events;

public class EventDetails
{
    public EventDetails(int capacity, string description)
    {
        Capacity = capacity;
        Description = description;
    }

    public Guid EventId { get; } = Guid.Empty;

    public int Capacity { get; private set; }

    public string Description { get; private set; }
}
