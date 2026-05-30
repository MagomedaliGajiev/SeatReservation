using CSharpFunctionalExtensions;
using Dapper;
using Microsoft.Extensions.Logging;
using SeatReservation.Application.Database;
using SeatReservation.Domain.Venues;
using SeatReservation.Infrastructure.Postgres.Database;
using SharedKernel;

namespace SeatReservation.Infrastructure.Postgres.Repositories;

public class NpgSqlVenuesRepository : IVenuesRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<NpgSqlVenuesRepository> _logger;

    public NpgSqlVenuesRepository(IDbConnectionFactory connectionFactory, ILogger<NpgSqlVenuesRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public Task<Result<Venue, Error>> GetById(VenueId id, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<Result<Venue, Error>> GetByIdWithSeats(VenueId id, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<IReadOnlyList<Venue>> GetByPrefix(string prefix, CancellationToken cancellationToken) => throw new NotImplementedException();

    public async Task<Result<Guid, Error>> Add(Venue venue, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var transaction = connection.BeginTransaction();

        try
        {
            const string venueInsertSql = """
                                          INSERT INTO venues (id, prefix, name, seats_limit)
                                          VALUES (@Id, @Prefix, @Name, @SeatsLimit)
                                          """;
            var venueInsertParams = new
            {
                Id = venue.Id.Value, Prefix = venue.Name.Prefix, Name = venue.Name.Name, SeatsLimit = venue.SeatsLimit,
            };

            await connection.ExecuteAsync(venueInsertSql, venueInsertParams);

            if (!venue.Seats.Any())
            {
                return venue.Id.Value;
            }

            const string seatsInsertSql = """
                                          INSERT INTO seats (id, row_number, seat_number, venue_id)
                                          VALUES (@Id, @RowNumber, @SeatNumber, @VenueId)
                                          """;

            var seatsInsertParams = venue.Seats.Select(s => new
            {
                Id = s.Id.Value, RowNumber = s.RowNumber, SeatNumber = s.SeatNumber, VenueId = venue.Id.Value,
            });

            await connection.ExecuteAsync(seatsInsertSql, seatsInsertParams);

            transaction.Commit();

            return venue.Id.Value;
        }
        catch (Exception ex)
        {
            transaction.Rollback();

            _logger.LogError(ex, "Fail to insert venue");

            return Error.Failure("venue.insert", "Fail to insert venue");
        }
    }

    public Task Save() => throw new NotImplementedException();

    public async Task<Result<Guid, Error>> UpdateVenueName(VenueId venueId, VenueName venueName, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var transaction = connection.BeginTransaction();

        try
        {
            const string updateNameSql = """
                                            UPDATE venues
                                            SET name = @Name
                                            WHERE id =  @Id
                                         """;

            var updateNameParams = new
            {
                Id = venueId.Value,
                Name = venueName.Name,
            };

            await connection.ExecuteAsync(updateNameSql, updateNameParams);

            transaction.Commit();

            return venueId.Value;
        }
        catch (Exception ex)
        {
            transaction.Rollback();

            _logger.LogError(ex, "Fail to update venue");

            return Error.Failure("venue.update", "Fail to update venue");
        }
    }

    public async Task<UnitResult<Error>> UpdateVenueNameByPrefix(
        string prefix,
        VenueName venueName,
        CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var transaction = connection.BeginTransaction();

        try
        {
            const string updateNameSql = """
                                            UPDATE venues
                                            SET name = @Name
                                            WHERE prefix LIKE  @Prefix
                                         """;

            var updateNameParams = new
            {
                Prefix = $"{prefix}%",
                Name = venueName.Name,
            };

            await connection.ExecuteAsync(updateNameSql, updateNameParams);

            transaction.Commit();

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            transaction.Rollback();

            _logger.LogError(ex, "Fail to update venue");

            return Error.Failure("venue.update", "Fail to update venue");
        }
    }

    public Task<UnitResult<Error>> DeleteSeatsByVenueId(VenueId venueId, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<UnitResult<Error>> AddSeats(IEnumerable<Seat> seats, CancellationToken cancellationToken) => throw new NotImplementedException();
}