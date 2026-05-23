using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeatReservation.Domain.Venues;
using SharedKernel;

namespace SeatReservation.Infrastructure.Postgres.Configurations;

public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("venues");

        builder.HasKey(v => v.Id).HasName("pk_venues");

        builder.Property(v => v.Id)
            .HasConversion(v => v.Value, id => new VenueId(id))
            .HasColumnName("id");

        builder.ComplexProperty(v => v.Name, nb =>
        {
            nb.Property(v => v.Prefix)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH50)
                .HasColumnName("prefix");

            nb.Property(v => v.Name)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH500)
                .HasColumnName("name");
        });

        builder.Property(v => v.SeatsLimit)
            .HasColumnName("seats_limit");

        builder
            .HasMany(v => v.Seats)
            .WithOne(s => s.Venue)
            .HasForeignKey("venue_id")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}