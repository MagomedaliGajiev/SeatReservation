using CSharpFunctionalExtensions;
using SeatReservation.Contracts.Events;
using SeatReservation.Domain.Events;
using SeatReservation.Domain.Venues;
using SharedKernel;

namespace SeatReservation.Application.Events.Commands;

public class CreateEventHandler
{
    private IEventsRepository _eventsRepository;

    public CreateEventHandler(IEventsRepository eventsRepository)
    {
        _eventsRepository = eventsRepository;
    }

    /// <summary>
    /// Этот метод создаёт событие нужного типа (концерт / конференция / онлайн)
    /// </summary>
    public async Task<Result<Guid, Error>> Handle(CreateEventRequest request, CancellationToken cancellationToken)
    {
        var venueId = new VenueId(request.VenueId);

        // создание доменной модели в зависимости от типа события
        Result<Event, Error> eventResult = request.Type switch
        {
            EventTypeDto.CONCERT => Event.CreateConcert(
                venueId,
                request.Name,
                request.EventDate,
                request.StartDate,
                request.EndDate,
                request.Capacity,
                request.Description,
                request.Performer!),

            EventTypeDto.CONFERENCE => Event.CreateConference(
                venueId,
                request.Name,
                request.EventDate,
                request.StartDate,
                request.EndDate,
                request.Capacity,
                request.Description,
                request.Speaker!,
                request.Topic!),

            EventTypeDto.ONLINE => Event.CreateOnline(
                venueId,
                request.Name,
                request.EventDate,
                request.StartDate,
                request.EndDate,
                request.Capacity,
                request.Description,
                request.Url!),

            _ => Error.Validation("event.type", "Unknown event type"),
        };

        if (eventResult.IsFailure)
        {
            return eventResult.Error;
        }

        // сохранение доменной модели в базу данных
        await _eventsRepository.Add(eventResult.Value, cancellationToken);

        return eventResult.Value.Id.Value;
    }
}