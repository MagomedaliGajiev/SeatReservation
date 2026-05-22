using CSharpFunctionalExtensions;
using SharedKernel;

namespace SeatReservation.Domain;

public record VenueName
{
    private VenueName(string prefix, string name)
    {
        Prefix = prefix;
        Name = name;
    }

    public string Prefix { get; }

    public string Name { get; }

    public override string ToString() => $"{Prefix}-{Name}";

    public static Result<VenueName, Error> Create(string prefix, string name)
    {
        if (string.IsNullOrWhiteSpace(prefix) || string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("venue.name", "venue name cannot be empty or whitespace");
        }

        if (prefix.Length > LengthConstants.LENGTH50 || name.Length > LengthConstants.LENGTH500)
        {
            return Error.Validation("venue.name", "venue name is too long");
        }

        return new VenueName(prefix, name);
    }
}