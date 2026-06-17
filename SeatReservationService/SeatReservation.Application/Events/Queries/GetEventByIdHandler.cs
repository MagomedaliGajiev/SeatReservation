using Microsoft.EntityFrameworkCore;
using SeatReservation.Application.Database;
using SeatReservation.Contracts.Events;
using SeatReservation.Contracts.Seats;
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
        return await _readDbContext.EventsRead
            .Include(e => e.Details)
            .Where(e => e.Id == new EventId(query.EventId))
            .Select(@event => new GetEventDto
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
                Seats = (from s in _readDbContext.SeatsRead
                    where s.VenueId == @event.VenueId
                    join rs in _readDbContext.ReservationSeatsRead
                        on new
                        {
                            SeatId = s.Id,
                            EventId = @event.Id,
                        }
                        equals new
                        {
                            SeatId = rs.SeatId,
                            EventId = rs.EventId,
                        }
                        into reservations
                    from r in reservations.DefaultIfEmpty()
                    orderby s.RowNumber, s.SeatNumber
                    select new AvailableSeatsDto
                    {
                        Id = s.Id.Value,
                        RowNumber = s.RowNumber,
                        SeatNumber = s.SeatNumber,
                        VenueId = s.VenueId.Value,
                        IsAvailable = r == null,
                    }).ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}