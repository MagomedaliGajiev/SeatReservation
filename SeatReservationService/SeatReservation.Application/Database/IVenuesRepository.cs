using CSharpFunctionalExtensions;
using SeatReservation.Domain.Venues;
using SharedKernel;

namespace SeatReservation.Application.Database;

public interface IVenuesRepository
{
    Task<Result<Venue, Error>> GetById(VenueId id, CancellationToken cancellationToken);

    Task<Result<Venue, Error>> GetByIdWithSeats(VenueId id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Venue>> GetByPrefix(string prefix, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> Add(Venue venue, CancellationToken cancellationToken = default);

    Task Save();

    Task<Result<Guid, Error>> UpdateVenueName(VenueId venueId, VenueName venueName, CancellationToken cancellationToken);

    Task<UnitResult<Error>> UpdateVenueNameByPrefix(string prefix, VenueName venueName, CancellationToken cancellationToken);

    Task<UnitResult<Error>> DeleteSeatsByVenueId(VenueId venueId, CancellationToken cancellationToken);
}