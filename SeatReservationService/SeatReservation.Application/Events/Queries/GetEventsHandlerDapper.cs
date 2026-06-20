using System.Data;
using Dapper;
using SeatReservation.Application.Database;
using SeatReservation.Contracts.Events;

namespace SeatReservation.Application.Events.Queries;

public class GetEventsHandlerDapper
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetEventsHandlerDapper(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<GetEventsDto> Handle(GetEventsRequest query, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();

        var conditions = new List<string>();

        // Поиск по названию
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            conditions.Add("e.name ILIKE @search");
            parameters.Add("search", $"%{query.Search}%");
        }


        // Фильтр по статусу
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            conditions.Add("LOWER(e.status) = LOWER(@status)");
            parameters.Add("status", query.Status);
        }

        // Фильтр по типу события
        if (!string.IsNullOrWhiteSpace(query.EventType))
        {
            conditions.Add("LOWER(e.type) = LOWER(@event_type)");
            parameters.Add("event_type", query.EventType);
        }

        // Фильтр по дате от
        if (query.DateFrom.HasValue)
        {
            conditions.Add("e.event_date >= @date_from");
            parameters.Add("date_from", DateTime.SpecifyKind(query.DateFrom.Value, DateTimeKind.Utc));
        }

        // Фильтр по дате до
        if (query.DateTo.HasValue)
        {
            // Включаем весь день DateTo, а не только полночь
            conditions.Add("e.event_date < @date_to");
            parameters.Add("date_to", DateTime.SpecifyKind(query.DateTo.Value, DateTimeKind.Utc).Date.AddDays(1));
        }

        // Фильтр по venue
        if (query.VenueId.HasValue)
        {
            conditions.Add("e.venue_id = @venue_id");
            parameters.Add("venue_id", query.VenueId.Value);
        }

        // Фильтр по min_available_seats 
        if (query.MinAvailableSeats.HasValue)
        {
            conditions.Add("""
                           ((SELECT COUNT(*) FROM seats s WHERE s.venue_id = e.venue_id) -
                           COALESCE((SELECT COUNT(*)
                                FROM reservation_seats rs
                                    JOIN reservations r ON rs.reservation_id = r.id
                                WHERE rs.event_id = e.id
                                  AND r.status IN ('CONFIRMED', 'PENDING')), 0)) >= @min_available_seats
                           """);
            parameters.Add("min_available_seats", query.MinAvailableSeats.Value);
        }

        // Pagination может не прийти из query string — подставляем значения по умолчанию.
        var pagination = query.Pagination ?? new PaginationRequest();

        // Параметры пагинации (всегда нужны)
        parameters.Add("offset", (pagination.Page - 1) * pagination.PageSize);
        parameters.Add("page_size", pagination.PageSize);

        // Строим WHERE clause
        string whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : string.Empty;

        // Общее количество событий, удовлетворяющих фильтрам (без учёта пагинации).
        long? totalCount = null;

        var command = new CommandDefinition(
            $"""
             SELECT e.id,
                   e.venue_id,
                   e.name,
                   e.event_date,
                   e.start_date,
                   e.end_date,
                   e.status,
                   e.type,
                   e.info,
                   ed.capacity,
                   ed.description,
                   ed.last_reservation_utc,
                   (SELECT COUNT(*)
                    FROM seats s
                    WHERE s.venue_id = e.venue_id) AS total_seats,

                   (SELECT COUNT(*)
                    FROM reservation_seats rs
                             JOIN reservations r ON rs.reservation_id = r.id
                    WHERE rs.event_id = e.id
                      AND r.status IN ('CONFIRMED', 'PENDING')) AS reserved_seats,

                   (SELECT COUNT(*)
                    FROM seats s
                    WHERE s.venue_id = e.venue_id) - (SELECT COUNT(*)
                                                      FROM reservation_seats rs
                                                               JOIN reservations r ON rs.reservation_id = r.id
                                                      WHERE rs.event_id = e.id
                                                        AND r.status IN ('CONFIRMED', 'PENDING')) AS available_seats,

                 COUNT(*) OVER() AS total_count
            FROM events e
                     JOIN events_details ed ON e.id = ed.event_id

            {whereClause}
            ORDER BY e.event_date DESC
            LIMIT @page_size OFFSET @offset
            """,
            parameters,
            cancellationToken: cancellationToken);

        var events = await connection.QueryAsync<EventDto, long, EventDto>(
            command,
            map: (@event, count) =>
            {
                totalCount ??= count;

                return @event;
            },
            splitOn: "total_count");

        return new GetEventsDto(events.ToList(), totalCount ?? 0);
    }
}