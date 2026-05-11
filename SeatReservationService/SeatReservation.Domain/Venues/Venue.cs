using CSharpFunctionalExtensions;
using SharedKernel;

namespace SeatReservation.Domain.Venues;

public class Venue
{
    private List<Seat> _seats = [];

    public Venue(Guid id, string name, int seatsLimit, IEnumerable<Seat> seats)
    {
        Id = id;
        Name = name;
        SeatsLimit = seatsLimit;
        _seats = _seats.ToList();
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    public int SeatsLimit { get; private set; }

    public int SeatsCount => _seats.Count;

    public IReadOnlyList<Seat> Seats => _seats;

    public UnitResult<Error> AddSeat(Seat seat)
    {
        if (SeatsCount >= SeatsLimit)
        {
            return Error.Conflict("venue.seats.limit", "");
        }

        _seats.Add(seat);

        return UnitResult.Success<Error>();
    }

    public void ExpandSeatsLimit(int seatsLimit) => SeatsLimit = seatsLimit;
}