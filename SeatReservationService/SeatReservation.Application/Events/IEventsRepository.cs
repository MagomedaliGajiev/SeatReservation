using CSharpFunctionalExtensions;
using SeatReservation.Domain.Events;
using SharedKernel;

namespace SeatReservation.Application.Events;

public interface IEventsRepository
{
    Task<Result<Event, Error>> GetById(EventId eventId, CancellationToken cancellationToken);
}