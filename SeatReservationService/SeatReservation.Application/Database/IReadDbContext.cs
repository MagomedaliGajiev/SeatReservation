using SeatReservation.Domain.Events;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Application.Database;

public interface IReadDbContext
{
    IQueryable<Event> EventsRead { get; }

    IQueryable<Venue> VenuesRead { get; }

    IQueryable<Seat> SeatsRead { get; }

    IQueryable<Reservation> ReservationsRead { get; }

    IQueryable<ReservationSeat> ReservationSeatsRead { get; }
}