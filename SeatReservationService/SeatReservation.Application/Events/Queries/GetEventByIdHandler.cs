using Microsoft.EntityFrameworkCore;
using SeatReservation.Application.Database;
using SeatReservation.Contracts.Events;
using SeatReservation.Domain.Events;

namespace SeatReservation.Application.Events.Queries;

public class GetEventByIdHandler
{
    private readonly IReadDbContext _readDbContext;

    public GetEventByIdHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<GetEventDto?> Handle(GetEventByIdRequest query, CancellationToken cancellationToken)
    {
        var @event = await _readDbContext.EventsRead
            .Include(e => e.Details)
            .FirstOrDefaultAsync(e => e.Id == new EventId(query.EventId), cancellationToken);

        if (@event is null)
        {
            return null;
        }

        return new GetEventDto()
        {
            Id = @event.Id.Value,
            Capacity = @event.Details.Capacity,
            Description = @event.Details.Description,
            LastReservationUtc = @event.Details.LastReservationUtc,
            EndDate = @event.EndDate,
            EventDate = @event.EventDate,
            Name = @event.Name,
            StartDate = @event.StartDate,
            Type = @event.Type.ToString(),
            VenueId = @event.VenueId.Value,
            Status = @event.Status.ToString(),
            Info = @event.Info.ToString(),
        };
    }
}