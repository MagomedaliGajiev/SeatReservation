using SeatReservation.Contracts.Events;
using SeatReservation.Domain.Events;

namespace SeatReservation.Application.Events;

public class GetEventByIdHandler
{
    private readonly IEventsRepository _eventsRepository;

    public GetEventByIdHandler(IEventsRepository eventsRepository)
    {
        _eventsRepository = eventsRepository;
    }

    public async Task<GetEventDto?> Handle(GetEventByIdRequest request, CancellationToken cancellation)
    {
        var @event = await _eventsRepository.GetById(new EventId(request.EventId), cancellation);

        if (@event is null)
        {
            return null;
        }

        return new GetEventDto()
        {
            Id = @event.Id.Value,
            Capacity = @event.Details.Capacity,
            Description = @event.Details.Description,
            EndDate = @event.EndDate,
            EventDate = @event.EventDate,
            Name = @event.Name,
            StartDate = @event.StartDate,
            Type = @event.Type.ToString(),
            VenueId = @event.VenueId.Value,
            Status = @event.Status.ToString(),
            Info = @event.Status.ToString(),
        };
    }
}