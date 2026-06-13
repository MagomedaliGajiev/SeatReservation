using SeatReservation.Domain.Venues;

namespace SeatReservation.Application.Venues;

public static class AdjacentSeatsFinder
{
    public static List<Seat> FindAdjacentSeatsInPreferredRow(
        IReadOnlyList<Seat> availableSeats,
        int requiredCount,
        int preferredRow)
    {
        if (requiredCount <= 0)
        {
            return [];
        }

        var seatsInRow = availableSeats
            .Where(s => s.RowNumber == preferredRow)
            .OrderBy(s => s.SeatNumber)
            .ToList();

        return FindAdjacentSeatsInRow(seatsInRow, requiredCount);
    }

    public static List<Seat> FindBestAdjacentSeats(IReadOnlyList<Seat> availableSeats, int requiredCount)
    {
        if (requiredCount <= 0 || availableSeats.Count < requiredCount)
        {
            return [];
        }

        var groupedByRow = availableSeats.GroupBy(s => s.RowNumber);

        foreach (var row in groupedByRow.OrderBy(g => g.Key))
        {
            var seatsInRow = row.OrderBy(s => s.SeatNumber).ToList();

            var adjacentSeats = FindAdjacentSeatsInRow(seatsInRow, requiredCount);

            if (adjacentSeats.Count == requiredCount)
            {
                return adjacentSeats;
            }
        }

        return [];
    }

    private static List<Seat> FindAdjacentSeatsInRow(List<Seat> seatsInRow, int requiredCount)
    {
        if (seatsInRow.Count < requiredCount)
        {
            return [];
        }

        for (int i = 0; i <= seatsInRow.Count - requiredCount; i++)
        {
            var candidateSeats = new List<Seat>();
            var isAdjacent = true;

            for (int j = 0; j < requiredCount; j++)
            {
                var currentSeat = seatsInRow[i + j];
                candidateSeats.Add(currentSeat);

                if (j > 0)
                {
                    var previousSeat = seatsInRow[i + j - 1];
                    if (currentSeat.SeatNumber != previousSeat.SeatNumber + 1)
                    {
                        isAdjacent = false;
                        break;
                    }
                }
            }

            if (isAdjacent)
            {
                return candidateSeats;
            }
        }

        return [];
    }
}
