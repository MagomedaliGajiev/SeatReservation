using CSharpFunctionalExtensions;
using SeatReservation.Domain.Venues;
using SharedKernel;

namespace SeatReservation.Application.Database;

public interface IVenuesRepository
{
    Task<Result<Guid, Error>> Add(Venue venue, CancellationToken cancellationToken = default);
}