using CSharpFunctionalExtensions;
using SeatReservation.Application.Database;
using SeatReservation.Contracts;
using SeatReservation.Domain.Venues;
using SharedKernel;

namespace SeatReservation.Application.Venues;

public class UpdateVenueSeatsHandler
{
    private readonly IVenuesRepository _repository;

    public UpdateVenueSeatsHandler(IVenuesRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid, Error>> Handle(UpdateVenueSeatsRequest request, CancellationToken cancellationToken)
    {
        var venueId = new VenueId(request.VenueId);

        var venue = await _repository.GetById(venueId, cancellationToken);
        if (venue.IsFailure)
        {
            return venue.Error;
        }

        List<Seat> seats = [];

        foreach (var seatRequest in request.Seats)
        {
            var seat = Seat.Create(venueId, seatRequest.RowNumber, seatRequest.SeatNumber);
            if (seat.IsFailure)
            {
                return seat.Error;
            }

            seats.Add(seat.Value);
        }

        venue.Value.UpdateSeats(seats);

        await _repository.DeleteSeatsByVenueId(venueId, cancellationToken);

        await _repository.Save();

        return venueId.Value;
    }
}