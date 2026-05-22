namespace SeatReservation.Domain;

public class User
{
    public User()
    {
    }

    public Details Details { get; set; }

    public Guid Id { get; set; }
}

public record Details
{
    public Details()
    {
    }

    public string Description { get; set; }

    public string FIO { get; set; }

    public IReadOnlyList<SocialNetwork> Socials { get; set; }
}

public record SocialNetwork
{
    public SocialNetwork()
    {
    }

    public string Name { get; init; }

    public string Link { get; init; }
}