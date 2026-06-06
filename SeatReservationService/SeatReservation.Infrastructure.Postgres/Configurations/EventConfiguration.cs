using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeatReservation.Domain.Events;
using SeatReservation.Domain.Venues;
using SeatReservation.Infrastructure.Postgres.Converters;

namespace SeatReservation.Infrastructure.Postgres.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");

        builder.HasKey(v => v.Id).HasName("pk_events");

        builder.Property(e => e.Id)
            .HasConversion(v => v.Value, id => new EventId(id))
            .HasColumnName("id");

        builder.Property(e => e.VenueId)
            .HasConversion(v => v.Value, id => new VenueId(id))
            .HasColumnName("venue_id");

        builder
            .HasOne<Venue>()
            .WithMany()
            .HasForeignKey(e => e.VenueId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(200);

        builder
            .Property(e => e.EventDate)
            .HasColumnName("event_date");

        builder
            .Property(e => e.StartDate)
            .HasColumnName("start_date");

        builder
            .Property(e => e.EndDate)
            .HasColumnName("end_date");

        builder
            .Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnName("status");

        builder
            .Property(e => e.Type)
            .HasConversion<string>()
            .HasColumnName("type");

        builder
            .Property(e => e.Info)
            .HasConversion<string>(new EventInfoConverter())
            .HasColumnName("info");
    }
}