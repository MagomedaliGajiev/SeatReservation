using Dapper;
using SeatReservation.Application.Database;
using SeatReservation.Contracts.Events;
using SeatReservation.Contracts.Seats;

namespace SeatReservation.Application.Events.Queries;

public class GetEventByIdHandlerDapper
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetEventByIdHandlerDapper(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<GetEventDto?> Handle(GetEventByIdRequest query, CancellationToken cancellationToken)
    {
        var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        GetEventDto? eventDto = null;

        var events = await connection.QueryAsync<GetEventDto, SeatsDto, GetEventDto>(
            """
                SELECT *,
                    e.id,
                    e.venue_id,
                    e.name,
                    e.type,
                    e.event_date,
                    e.start_date,
                    e.end_date,
                    e.status,
                    e.info,
                    ed.capacity,
                    ed.description,
                    s.id,
                    s.venue_id,
                    s.row_number,
                    s.seat_number
                FROM events e
                JOIN events_details ed ON ed.event_id = e.id
                JOIN seats s ON e.venue_id = s.venue_id
                WHERE e.id = @eventId
                ORDER BY s.row_number, s.seat_number
            """,
            param: new
            {
                eventId = query.EventId,
            },
            splitOn: "id",
            map: (e, s) =>
            {
                eventDto ??= e;

                eventDto.Seats.Add(s);

                return eventDto;
            });

        return eventDto;
    }
}