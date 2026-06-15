namespace SeatReservation.Infrastructure.Postgres.Seeding;

/// <summary>
/// Управляет объёмом данных, которые будут засидированы.
/// Меняй значения, чтобы регулировать количество записей в каждой таблице.
/// </summary>
public static class SeedingConstants
{
    /// <summary>Сколько пользователей создать.</summary>
    public const int USERS_COUNT = 500;

    /// <summary>Сколько площадок создать.</summary>
    public const int VENUES_COUNT = 500;

    /// <summary>Сколько рядов мест в каждой площадке.</summary>
    public const int SEAT_ROWS_PER_VENUE = 10;

    /// <summary>Сколько мест в каждом ряду.</summary>
    public const int SEATS_PER_ROW = 15;

    /// <summary>Сколько мероприятий создать (распределяются по случайным площадкам).</summary>
    public const int EVENTS_COUNT = 3000;

    /// <summary>Максимум бронирований на одно мероприятие (фактическое число — случайное от 0 до этого значения).</summary>
    public const int MAX_RESERVATIONS_PER_EVENT = 8;

    /// <summary>Максимум мест в одном бронировании (фактическое число — случайное от 1 до этого значения).</summary>
    public const int MAX_SEATS_PER_RESERVATION = 5;
}