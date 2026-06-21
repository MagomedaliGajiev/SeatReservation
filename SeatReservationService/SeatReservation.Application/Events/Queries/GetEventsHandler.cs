using Microsoft.EntityFrameworkCore;
using SeatReservation.Application.Database;
using SeatReservation.Contracts.Events;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Application.Events.Queries;

public class GetEventsHandler
{
    private readonly IReadDbContext _readDbContext;

    public GetEventsHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<GetEventsDto> Handle(GetEventsRequest query, CancellationToken cancellationToken)
    {
        var eventsQuery = _readDbContext.EventsRead;

        if (!string.IsNullOrEmpty(query.Search))
            eventsQuery = eventsQuery.Where(e => EF.Functions.Like(e.Name.ToLower(), $"%{query.Search.ToLower()}%"));

        if (!string.IsNullOrEmpty(query.EventType))
            eventsQuery = eventsQuery.Where(e => e.Type.ToString().ToLower() == query.EventType.ToLower());

        if (query.DateFrom.HasValue)
        {
            var dateFrom = DateTime.SpecifyKind(query.DateFrom.Value, DateTimeKind.Utc);
            eventsQuery = eventsQuery.Where(e => e.EventDate >= dateFrom);
        }

        if (query.DateTo.HasValue)
        {
            // Включаем весь день DateTo, а не только полночь
            var dateTo = DateTime.SpecifyKind(query.DateTo.Value, DateTimeKind.Utc).Date.AddDays(1);
            eventsQuery = eventsQuery.Where(e => e.EventDate < dateTo);
        }

        if (!string.IsNullOrEmpty(query.Status))
            eventsQuery = eventsQuery.Where(e => e.Status.ToString().ToLower() == query.Status.ToLower());

        if (query.VenueId.HasValue)
            eventsQuery = eventsQuery.Where(e => e.VenueId == new VenueId(query.VenueId.Value));

        if (query.MinAvailableSeats.HasValue)
        {
            eventsQuery = eventsQuery.Where(e =>
                _readDbContext.SeatsRead.Count(s => s.VenueId == e.VenueId) -
                _readDbContext.ReservationSeatsRead.Count(rs =>
                    rs.EventId == e.Id &&
                    (rs.Reservation.Status == ReservationStatus.CONFIRMED ||
                     rs.Reservation.Status == ReservationStatus.PENDING))
                >= query.MinAvailableSeats.Value);
        }

        long totalCount = await eventsQuery.LongCountAsync(cancellationToken);

        // Проецируем нужные поля прямо в SQL. Details читаем здесь же в проекции
        // (иначе при проекции Include игнорируется и Details был бы null), а
        // Type/Status/Info берём строками — как они хранятся в БД, чтобы по ним
        // можно было сортировать прямо в запросе.
        var projectedQuery = eventsQuery
            .Select(e => new EventRow
            {
                Id = e.Id.Value,
                Capacity = e.Details.Capacity,
                Description = e.Details.Description,
                EndDate = e.EndDate,
                EventDate = e.EventDate,
                Name = e.Name,
                StartDate = e.StartDate,
                Type = e.Type.ToString(),
                VenueId = e.VenueId.Value,
                Status = e.Status.ToString(),
                Info = e.Info.ToString(),
                TotalSeats = _readDbContext.SeatsRead.Count(s => s.VenueId == e.VenueId),
                ReservedSeats = _readDbContext.ReservationSeatsRead.Count(rs => rs.EventId == e.Id &&
                    (rs.Reservation.Status == ReservationStatus.CONFIRMED ||
                     rs.Reservation.Status == ReservationStatus.PENDING)),
            });

        var ascending = query.SortDirection?.ToLower() == "asc";
        var sortBy = query.SortBy?.ToLower();
        var skip = (query.Pagination.Page - 1) * query.Pagination.PageSize;
        var take = query.Pagination.PageSize;

        // Доля занятых мест. Вторичный ключ (Id) делает порядок устойчивым — иначе
        // при равных значениях строки перескакивают между страницами при пагинации.
        static double Popularity(EventRow x) => x.TotalSeats == 0 ? 0 : (double)x.ReservedSeats / x.TotalSeats;

        List<EventRow> rows;

        if (sortBy == "popularity")
        {
            // Сортировка по популярности — это деление двух коррелированных подзапросов,
            // которое EF не умеет переводить в ORDER BY. Поэтому сортируем и пагинируем
            // в памяти (Dapper-вариант делает это в SQL через CTE).
            var all = await projectedQuery.ToListAsync(cancellationToken);

            rows = (ascending
                    ? all.OrderBy(Popularity).ThenBy(x => x.Id)
                    : all.OrderByDescending(Popularity).ThenBy(x => x.Id))
                .Skip(skip)
                .Take(take)
                .ToList();
        }
        else
        {
            projectedQuery = sortBy switch
            {
                "name" => ascending
                    ? projectedQuery.OrderBy(x => x.Name).ThenBy(x => x.Id)
                    : projectedQuery.OrderByDescending(x => x.Name).ThenBy(x => x.Id),
                "status" => ascending
                    ? projectedQuery.OrderBy(x => x.Status).ThenBy(x => x.Id)
                    : projectedQuery.OrderByDescending(x => x.Status).ThenBy(x => x.Id),
                "type" => ascending
                    ? projectedQuery.OrderBy(x => x.Type).ThenBy(x => x.Id)
                    : projectedQuery.OrderByDescending(x => x.Type).ThenBy(x => x.Id),
                _ => ascending
                    ? projectedQuery.OrderBy(x => x.EventDate).ThenBy(x => x.Id)
                    : projectedQuery.OrderByDescending(x => x.EventDate).ThenBy(x => x.Id),
            };

            rows = await projectedQuery
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        var events = rows.Select(x => new EventDto
        {
            Id = x.Id,
            Capacity = x.Capacity,
            Description = x.Description,
            EndDate = x.EndDate,
            EventDate = x.EventDate,
            Name = x.Name,
            StartDate = x.StartDate,
            Type = x.Type,
            VenueId = x.VenueId,
            Status = x.Status,
            Info = x.Info,
            TotalSeats = x.TotalSeats,
            ReservedSeats = x.ReservedSeats,
            AvailableSeats = x.TotalSeats - x.ReservedSeats,
            PopularityPercentage = Math.Round(Popularity(x) * 100, 2),
        }).ToList();

        return new GetEventsDto(events, totalCount);
    }

    private sealed record EventRow
    {
        public Guid Id { get; init; }
        public int Capacity { get; init; }
        public string Description { get; init; } = string.Empty;
        public DateTime EndDate { get; init; }
        public DateTime EventDate { get; init; }
        public string Name { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public string Type { get; init; } = string.Empty;
        public Guid VenueId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string Info { get; init; } = string.Empty;
        public int TotalSeats { get; init; }
        public int ReservedSeats { get; init; }
    }
}
