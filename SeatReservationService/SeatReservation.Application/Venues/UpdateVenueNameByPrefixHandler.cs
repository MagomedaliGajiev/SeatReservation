using CSharpFunctionalExtensions;
using SeatReservation.Application.Database;
using SeatReservation.Contracts;
using SeatReservation.Domain.Venues;
using SharedKernel;

namespace SeatReservation.Application.Venues;

public class UpdateVenueNameByPrefixHandler
{
    private readonly IVenuesRepository _repository;

    public UpdateVenueNameByPrefixHandler(IVenuesRepository repository)
    {
        _repository = repository;
    }

    public async Task<UnitResult<Error>> Handle(UpdateVenueNameByPrefixRequest request, CancellationToken cancellationToken)
    {
        var venueName = VenueName.CreateWithoutPrefix(request.Name);

        if (venueName.IsFailure)
        {
            return venueName.Error;
        }

        var result = await _repository.UpdateVenueNameByPrefix(request.Prefix, venueName.Value, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error;
        }

        return UnitResult.Success<Error>();
    }
}