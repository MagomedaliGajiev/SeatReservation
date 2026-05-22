using CSharpFunctionalExtensions;
using SharedKernel;

namespace SeatReservation.Domain.Venues;

public record VenueId(Guid Value);

public class Venue
{
    private List<Seat> _seats = [];

    // EF Core
    private Venue()
    {
    }

    public Venue(VenueId id, VenueName name, int seatsLimit, IEnumerable<Seat> seats)
    {
        Id = id;
        Name = name;
        SeatsLimit = seatsLimit;
        _seats = seats.ToList();
    }

    public VenueId Id { get; } = null!;

    public VenueName Name { get; private set; }

    public int SeatsLimit { get; private set; }

    public int SeatsCount => _seats.Count;

    public IReadOnlyList<Seat> Seats => _seats;

    public UnitResult<Error> AddSeat(Seat seat)
    {
        if (SeatsCount >= SeatsLimit)
        {
            return Error.Conflict("venue.seats.limit", string.Empty);
        }

        _seats.Add(seat);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> UpdateSeats(IEnumerable<Seat> seats)
    {
        var seatsList = seats.ToList();

        if (seatsList.Count > SeatsLimit)
        {
            return Error.Failure("venue.seats.limit", "There are too many seats");
        }

        _seats = seatsList.ToList();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> UpdateName(string name)
    {
        var newVenueName = VenueName.Create(Name.Prefix, name);
        if (newVenueName.IsFailure)
        {
            return newVenueName.Error;
        }

        Name = newVenueName.Value;

        return UnitResult.Success<Error>();
    }

    public void ExpandSeatsLimit(int seatsLimit) => SeatsLimit = seatsLimit;

    public static Result<Venue, Error> Create(
        string prefix,
        string name,
        int seatsLimit,
        IEnumerable<Seat> seats)
    {
        if (seatsLimit <= 0)
        {
            return Error.Validation("venue.seatsLimit", "Seats limit must be greater than zero");
        }

        var venueNameResult = VenueName.Create(prefix, name);
        if (venueNameResult.IsFailure)
        {
            return venueNameResult.Error;
        }

        var venueSeats = seats.ToList();

        if (venueSeats.Count < 1)
        {
            return Error.Validation("venue.seats", "Number of seats can not be zero");
        }

        if (venueSeats.Count > seatsLimit)
        {
            return Error.Validation("venue.seats", "Number of seats exceeds the venue's seat limit");
        }

        return new Venue(new VenueId(Guid.NewGuid()), venueNameResult.Value, seatsLimit, venueSeats);
    }
}