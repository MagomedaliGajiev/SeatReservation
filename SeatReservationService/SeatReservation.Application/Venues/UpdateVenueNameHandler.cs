using CSharpFunctionalExtensions;
using SeatReservation.Application.Database;
using SeatReservation.Contracts;
using SeatReservation.Domain.Venues;
using SharedKernel;

namespace SeatReservation.Application.Venues;

public class UpdateVenueNameHandler
{
    private readonly IVenuesRepository _repository;

    public UpdateVenueNameHandler(IVenuesRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid, Error>> Handle(UpdateVenueNameRequest request, CancellationToken cancellationToken)
    {
        var venueId = new VenueId(request.Id);

        (_, bool isFailure, Venue? venue, Error? error) = await _repository.GetById(venueId, cancellationToken);

        if (isFailure)
        {
            return error;
        }

        venue.UpdateName(request.Name);

        await _repository.Save();

        return venueId.Value;
    }
}