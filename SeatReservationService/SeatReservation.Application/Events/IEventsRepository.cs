using CSharpFunctionalExtensions;
using SeatReservation.Domain.Events;
using SharedKernel;

namespace SeatReservation.Application.Events;

public interface IEventsRepository
{
    Task<Result<Event, Error>> GetByIdWithLock(EventId eventId, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> Add(Event @event, CancellationToken cancellationToken = default);
}