using System.Data;

namespace SeatReservation.Infrastructure.Postgres.Database;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}