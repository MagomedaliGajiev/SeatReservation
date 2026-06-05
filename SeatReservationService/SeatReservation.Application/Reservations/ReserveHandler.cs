using CSharpFunctionalExtensions;
using SeatReservation.Contracts;
using SharedKernel;

namespace SeatReservation.Application.Reservations;

public class ReserveHandler
{
    public async Task<Result<Guid, Error>> Handle(ReserveRequest request, CancellationToken cancellationToken)
    {
        // Бронирование мест на мероприятии
        
        // Создать Reservation с ReservedSeats
        
        //
        
    }
}