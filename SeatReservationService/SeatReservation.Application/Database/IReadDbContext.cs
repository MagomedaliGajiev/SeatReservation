using SeatReservation.Domain.Events;

namespace SeatReservation.Application.Database;

public interface IReadDbContext
{
    IQueryable<Event> EventsRead { get; }
}