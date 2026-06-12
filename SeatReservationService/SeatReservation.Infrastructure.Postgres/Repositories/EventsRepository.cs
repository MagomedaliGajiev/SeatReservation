using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeatReservation.Application.Events;
using SeatReservation.Domain.Events;
using SharedKernel;
using EventId = SeatReservation.Domain.Events.EventId;

namespace SeatReservation.Infrastructure.Postgres.Repositories;

public class EventsRepository : IEventsRepository
{
    private readonly ReservationServiceDbContext _dbContext;
    private readonly ILogger<EventsRepository> _logger;

    public EventsRepository(ReservationServiceDbContext dbContext, ILogger<EventsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Event, Error>> GetById(EventId eventId, CancellationToken cancellationToken)
    {
        var @event = await _dbContext.Events
            .Include(e => e.Details)
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        if (@event is null)
        {
            return Error.Failure("event.not.found", "Event not found");
        }

        return @event;
    }

    public async Task<Result<Guid, Error>> Add(Event @event, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Events.AddAsync(@event, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return @event.Id.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fail to insert event");

            return Error.Failure("event.insert", "Fail to insert event");
        }
    }
}