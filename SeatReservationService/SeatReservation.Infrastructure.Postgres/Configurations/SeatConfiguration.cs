using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Infrastructure.Postgres.Configurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("seats");

        builder.HasKey(s => s.Id).HasName("pk_seats");

        builder.Property(v => v.Id)
            .HasConversion(s => s.Value, id => new SeatId(id))
            .HasColumnName("id");

        builder.Property(s => s.VenueId)
            .HasConversion(v => v.Value, id => new VenueId(id))
            .HasColumnName("venue_id");

        builder
            .Property(s => s.RowNumber)
            .HasColumnName("row_number");

        builder
            .Property(s => s.SeatNumber)
            .HasColumnName("seat_number");
    }
}