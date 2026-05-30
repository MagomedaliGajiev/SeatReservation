using CSharpFunctionalExtensions;
using SeatReservation.Application.Database;
using SeatReservation.Contracts;
using SeatReservation.Domain.Venues;
using SharedKernel;

namespace SeatReservation.Application.Venues;

public class CreateVenueHandler
{
    private readonly IVenuesRepository _venuesRepository;

    public CreateVenueHandler(IVenuesRepository venuesRepository)
    {
        _venuesRepository = venuesRepository;
    }

    /// <summary>
    /// Этот метод создает плаащадку со всеми местами
    /// </summary>
    public async Task<Result<Guid, Error>> Handle(CreateVenueRequest request, CancellationToken cancellationToken)
    {
        // валидация входных параметров

        // бизнес валидация

        // создание доменных моделей
        var venue = Venue.Create(request.Prefix, request.Name, request.SeatsLimit);
        if (venue.IsFailure)
        {
            return venue.Error;
        }

        List<Seat> seats = [];
        foreach (var seatRequest in request.Seats)
        {
            var seat = Seat.Create(venue.Value, seatRequest.RowNumber, seatRequest.SeatNumber);

            if (seat.IsFailure)
            {
                return seat.Error;
            }

            seats.Add(seat.Value);
        }

        venue.Value.AddSeats(seats);

        // сохранение доменных моделей в базу данных
        await _venuesRepository.Add(venue.Value, cancellationToken);

        return venue.Value.Id.Value;
    }
}