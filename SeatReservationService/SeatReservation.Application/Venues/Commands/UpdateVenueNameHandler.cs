using CSharpFunctionalExtensions;
using SeatReservation.Application.Database;
using SeatReservation.Contracts.Venues;
using SeatReservation.Domain.Venues;
using SharedKernel;

namespace SeatReservation.Application.Venues.Commands;

public class UpdateVenueNameHandler
{
    private readonly IVenuesRepository _repository;
    private readonly ITransactionManager _transactionManager;

    public UpdateVenueNameHandler(IVenuesRepository repository, ITransactionManager transactionManager)
    {
        _repository = repository;
        _transactionManager = transactionManager;
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

        await _transactionManager.SaveChangesAsync(cancellationToken);

        return venueId.Value;
    }
}