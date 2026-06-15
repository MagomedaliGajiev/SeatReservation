namespace SeatReservation.Contracts.Seats;

public record CreateSeatRequest(int RowNumber, int SeatNumber)
{
}