using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeatReservation.Domain;
using SeatReservation.Domain.Events;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Infrastructure.Postgres.Seeding;

public class ReservationSeeder : ISeeder
{
    private readonly ReservationServiceDbContext _context;
    private readonly ILogger<ReservationSeeder> _logger;
    private readonly RandomDataGenerator _faker = new();

    public ReservationSeeder(ReservationServiceDbContext context, ILogger<ReservationSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting seeding reservation data...");

        // Гарантируем, что схема существует, до любых обращений к таблицам.
        // Миграции нельзя выполнять внутри открытой транзакции, поэтому это идёт первым.
        await _context.Database.MigrateAsync();

        // Массовая вставка десятков тысяч сущностей: автодетект изменений на каждый
        // AddRange сильно замедляет процесс — DetectChanges выполнится один раз в SaveChanges.
        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await ClearDatabaseAsync();
            await SeedDataAsync();

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Finishing seeding reservations data.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "An error occured while seeding reservations data. Changes were rolled back.");
            throw;
        }
        finally
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = true;
        }
    }

    /// <summary>
    /// Полностью очищает все таблицы в порядке от дочерних к родительским.
    /// Выполняется внутри той же транзакции, что и наполнение.
    /// </summary>
    private async Task ClearDatabaseAsync()
    {
        await _context.Set<ReservationSeat>().ExecuteDeleteAsync();
        await _context.Set<Reservation>().ExecuteDeleteAsync();
        await _context.Set<EventDetails>().ExecuteDeleteAsync();
        await _context.Set<Event>().ExecuteDeleteAsync();
        await _context.Set<Seat>().ExecuteDeleteAsync();
        await _context.Set<Venue>().ExecuteDeleteAsync();
        await _context.Set<User>().ExecuteDeleteAsync();
    }

    /// <summary>
    /// Генерирует граф объектов и добавляет его в контекст.
    /// Реальная запись в БД происходит в <see cref="SeedAsync"/> через единый SaveChanges.
    /// </summary>
    private async Task SeedDataAsync()
    {
        var users = BuildUsers();
        await _context.Users.AddRangeAsync(users);

        var venues = BuildVenues();
        await _context.Venues.AddRangeAsync(venues);

        var seatsByVenue = venues.ToDictionary(v => v.Id, v => v.Seats);

        var events = BuildEvents(venues);
        await _context.Events.AddRangeAsync(events);

        var reservations = BuildReservations(events, users, seatsByVenue);
        await _context.Reservations.AddRangeAsync(reservations);

        _logger.LogInformation(
            "Generated {Users} users, {Venues} venues, {Events} events, {Reservations} reservations.",
            users.Count,
            venues.Count,
            events.Count,
            reservations.Count);
    }

    private List<User> BuildUsers()
    {
        var users = new List<User>(SeedingConstants.USERS_COUNT);

        for (var i = 0; i < SeedingConstants.USERS_COUNT; i++)
        {
            users.Add(new User
            {
                Id = Guid.NewGuid(),
                Details = _faker.UserDetails(),
            });
        }

        return users;
    }

    private List<Venue> BuildVenues()
    {
        var venues = new List<Venue>(SeedingConstants.VENUES_COUNT);
        var seatsLimit = SeedingConstants.SEAT_ROWS_PER_VENUE * SeedingConstants.SEATS_PER_ROW;

        for (var i = 0; i < SeedingConstants.VENUES_COUNT; i++)
        {
            var venueResult = Venue.Create(_faker.VenuePrefix(), _faker.VenueName(), seatsLimit);
            if (venueResult.IsFailure)
            {
                throw new InvalidOperationException($"Failed to generate venue: {venueResult.Error}");
            }

            var venue = venueResult.Value;
            venue.AddSeats(BuildSeats(venue.Id));
            venues.Add(venue);
        }

        return venues;
    }

    private List<Seat> BuildSeats(VenueId venueId)
    {
        var seats = new List<Seat>(SeedingConstants.SEAT_ROWS_PER_VENUE * SeedingConstants.SEATS_PER_ROW);

        for (var row = 1; row <= SeedingConstants.SEAT_ROWS_PER_VENUE; row++)
        {
            for (var number = 1; number <= SeedingConstants.SEATS_PER_ROW; number++)
            {
                var seatResult = Seat.Create(venueId, row, number);
                if (seatResult.IsFailure)
                {
                    throw new InvalidOperationException($"Failed to generate seat: {seatResult.Error}");
                }

                seats.Add(seatResult.Value);
            }
        }

        return seats;
    }

    private List<Event> BuildEvents(IReadOnlyList<Venue> venues)
    {
        var events = new List<Event>(SeedingConstants.EVENTS_COUNT);

        for (var i = 0; i < SeedingConstants.EVENTS_COUNT; i++)
        {
            var venue = _faker.Pick(venues);
            events.Add(BuildEvent(venue.Id));
        }

        return events;
    }

    private Event BuildEvent(VenueId venueId)
    {
        var name = _faker.EventName();
        var description = _faker.Description();
        var capacity = _faker.Next(50, 1000);

        // Все даты должны быть в будущем — этого требует доменная валидация Event.
        var eventDate = DateTime.UtcNow.AddDays(_faker.Next(10, 180));
        var startDate = eventDate.AddHours(_faker.Next(1, 8));
        var endDate = startDate.AddHours(_faker.Next(2, 6));

        var eventResult = _faker.Next(0, 2) switch
        {
            0 => Event.CreateConcert(venueId, name, eventDate, startDate, endDate, capacity, description, _faker.PersonName()),
            1 => Event.CreateConference(venueId, name, eventDate, startDate, endDate, capacity, description, _faker.PersonName(), _faker.Topic()),
            _ => Event.CreateOnline(venueId, name, eventDate, startDate, endDate, capacity, description, _faker.OnlineUrl()),
        };

        if (eventResult.IsFailure)
        {
            throw new InvalidOperationException($"Failed to generate event: {eventResult.Error}");
        }

        return eventResult.Value;
    }

    private List<Reservation> BuildReservations(
        IReadOnlyList<Event> events,
        IReadOnlyList<User> users,
        IReadOnlyDictionary<VenueId, IReadOnlyList<Seat>> seatsByVenue)
    {
        var reservations = new List<Reservation>();

        foreach (var @event in events)
        {
            if (!seatsByVenue.TryGetValue(@event.VenueId, out var venueSeats) || venueSeats.Count == 0)
            {
                continue;
            }

            // Перемешиваем места площадки и раздаём их без повторов:
            // уникальный индекс (event_id, seat_id) запрещает бронировать одно место дважды на мероприятие.
            var pool = venueSeats.ToList();
            _faker.Shuffle(pool);

            var reservationsForEvent = _faker.Next(0, SeedingConstants.MAX_RESERVATIONS_PER_EVENT);
            var offset = 0;

            for (var i = 0; i < reservationsForEvent && offset < pool.Count; i++)
            {
                var take = Math.Min(_faker.Next(1, SeedingConstants.MAX_SEATS_PER_RESERVATION), pool.Count - offset);
                var seatIds = pool.GetRange(offset, take).Select(seat => seat.Id.Value).ToList();
                offset += take;

                var user = _faker.Pick(users);

                var reservationResult = Reservation.Create(@event.Id, user.Id, seatIds);
                if (reservationResult.IsFailure)
                {
                    throw new InvalidOperationException($"Failed to generate reservation: {reservationResult.Error}");
                }

                reservations.Add(reservationResult.Value);
            }
        }

        return reservations;
    }
}