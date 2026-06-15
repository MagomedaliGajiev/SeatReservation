using SeatReservation.Domain;

namespace SeatReservation.Infrastructure.Postgres.Seeding;

/// <summary>
/// Генерирует случайные шаблонные данные из заранее заданных наборов слов.
/// Конкретные записи не хардкодятся — каждая собирается случайной комбинацией шаблонов.
/// </summary>
internal sealed class RandomDataGenerator
{
    private static readonly string[] VenuePrefixes =
        ["MSK", "SPB", "NYC", "LDN", "BER", "PAR", "TOK", "DXB", "ROM", "AMS"];

    private static readonly string[] VenueAdjectives =
        ["Grand", "Royal", "Central", "Golden", "Silver", "Crystal", "Imperial", "Modern", "Old", "New"];

    private static readonly string[] VenueNouns =
        ["Hall", "Arena", "Theatre", "Stadium", "Palace", "Club", "Center", "Dome", "Stage", "Plaza"];

    private static readonly string[] EventAdjectives =
        ["Annual", "International", "Summer", "Winter", "Open", "Live", "Acoustic", "Digital", "Global", "Spring"];

    private static readonly string[] EventNouns =
        ["Festival", "Concert", "Show", "Summit", "Forum", "Meetup", "Gala", "Session", "Tour", "Conference"];

    private static readonly string[] FirstNames =
        ["Alex", "Maria", "John", "Elena", "David", "Anna", "Michael", "Sofia", "Daniel", "Olga", "Ivan", "Kate"];

    private static readonly string[] LastNames =
        ["Smith", "Ivanov", "Brown", "Petrov", "Johnson", "Sidorov", "Williams", "Kuznetsov", "Miller", "Popov"];

    private static readonly string[] Patronymics =
        ["Ivanovich", "Petrovna", "Sergeevich", "Alekseevna", "Dmitrievich", "Andreevna", "Olegovich", "Pavlovna"];

    private static readonly string[] Topics =
        [".NET Performance", "Distributed Systems", "Cloud Architecture", "Machine Learning", "Cybersecurity", "DevOps Culture", "Microservices", "Event Sourcing"];

    private static readonly string[] SocialNames =
        ["Telegram", "GitHub", "LinkedIn", "Twitter", "Instagram", "Facebook"];

    private static readonly string[] DescriptionParts =
        [
            "An unforgettable experience for everyone.",
            "Join us for a remarkable evening.",
            "A unique gathering of talented people.",
            "The best place to be this season.",
            "Limited seats available — book early.",
            "Featuring world-class performers and speakers.",
            "A perfect mix of culture and entertainment.",
        ];

    private readonly Random _random = new();

    /// <summary>Случайное целое в диапазоне [min; maxInclusive].</summary>
    public int Next(int min, int maxInclusive) => _random.Next(min, maxInclusive + 1);

    /// <summary>Случайный элемент списка.</summary>
    public T Pick<T>(IReadOnlyList<T> items) => items[_random.Next(items.Count)];

    /// <summary>Перемешивает список на месте (Fisher-Yates).</summary>
    public void Shuffle<T>(IList<T> items)
    {
        for (var i = items.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }

    public string VenuePrefix() => Pick(VenuePrefixes);

    public string VenueName() => $"{Pick(VenueAdjectives)} {Pick(VenueNouns)}";

    public string EventName() => $"{Pick(EventAdjectives)} {Pick(EventNouns)} {Next(2026, 2030)}";

    public string PersonName() => $"{Pick(FirstNames)} {Pick(LastNames)}";

    public string Topic() => Pick(Topics);

    public string OnlineUrl() => $"https://stream.example.com/{Guid.NewGuid():N}";

    public string Description()
    {
        var sentences = Enumerable
            .Range(0, Next(1, 3))
            .Select(_ => Pick(DescriptionParts));

        return string.Join(' ', sentences);
    }

    public Details UserDetails() => new()
    {
        FIO = $"{Pick(LastNames)} {Pick(FirstNames)} {Pick(Patronymics)}",
        Description = Description(),
        Socials = GenerateSocials(),
    };

    private IReadOnlyList<SocialNetwork> GenerateSocials()
    {
        var count = Next(0, 3);
        var socials = new List<SocialNetwork>(count);

        for (var i = 0; i < count; i++)
        {
            var name = Pick(SocialNames);
            socials.Add(new SocialNetwork
            {
                Name = name,
                Link = $"https://{name.ToLowerInvariant()}.com/{Guid.NewGuid():N}",
            });
        }

        return socials;
    }
}