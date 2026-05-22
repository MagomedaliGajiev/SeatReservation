using CSharpFunctionalExtensions;
using SeatReservation.Application.Database;
using SeatReservation.Contracts;
using SeatReservation.Domain.Venues;
using SharedKernel;

namespace SeatReservation.Application;

public class CreateVenueHandler
{
    private readonly IReservationDbContext _dbContext;

    public CreateVenueHandler(IReservationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Этот метод создает плаащадку со всеми местами
    /// </summary>
    public async Task<Result<Guid, Error>> Handle(CreateVenueRequest request, CancellationToken cancellationToken)
    {
        // валидация входных параметров

        // бизнес валидация

        // создание доменных моделей
        List<Seat> seats = [];
        foreach (var seatRequest in request.Seats)
        {
            var seat = Seat.Create(seatRequest.RowNumber, seatRequest.SeatNumber);

            if (seat.IsFailure)
            {
                return seat.Error;
            }

            seats.Add(seat.Value);
        }

        var venue = Venue.Create(request.Prefix, request.Name, request.SeatsLimit, seats);

        // сохранение доменных моделей в базу данных
        var entries1 = _dbContext.ChangeTracker.Entries();

        await _dbContext.Venues.AddAsync(venue.Value, cancellationToken);

        var entries2 = _dbContext.ChangeTracker.Entries();

        await _dbContext.SaveChangesAsync(cancellationToken);

        var entries3 = _dbContext.ChangeTracker.Entries();

        return venue.Value.Id.Value;
    }
}