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

        string direction = query.SortDirection?.ToLower() == "asc" ? "ASC" : "DESC";

        var orderByField = query.SortBy?.ToLower() switch
        {
            "date" => "event_date",
            "name" => "name",
            "status" => "status",
            "type" => "type",
            "popularity" => "popularity_percentage",
            _ => "event_date",
        };

        // Вторичный ключ (id) делает порядок устойчивым при равных значениях —
        // иначе строки могут перескакивать между страницами при пагинации.
        string orderByClause = $"ORDER BY {orderByField} {direction}, id ASC";

        // Общее количество событий, удовлетворяющих фильтрам (без учёта пагинации).
        long? totalCount = null;

        var command = new CommandDefinition(
            $"""
             WITH event_stats AS (SELECT e.id,
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
                                         WHERE s.venue_id = e.venue_id)              AS total_seats,
            
                                        (SELECT COUNT(*)
                                         FROM reservation_seats rs
                                                  JOIN reservations r ON rs.reservation_id = r.id AND event_id = e.id
                                           WHERE r.status IN ('CONFIRMED', 'PENDING')) AS reserved_seats,
            
                                        COUNT(*) OVER ()                             AS total_count
                                 FROM events e
                                          JOIN events_details ed ON e.id = ed.event_id
                                          {whereClause})
            
            SELECT id,
                   venue_id,
                   name,
                   event_date,
                   start_date,
                   end_date,
                   status,
                   type,
                   info,
                   capacity,
                   description,
                   total_seats,
                   reserved_seats,
                   total_seats - reserved_seats                          as available_seats,
                   ROUND(COALESCE(reserved_seats::decimal / NULLIF(total_seats, 0) * 100, 0), 2) AS popularity_percentage,
                   total_count
            FROM event_stats
            {orderByClause}
            LIMIT @page_size OFFSET @offset;
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