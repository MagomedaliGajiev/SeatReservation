using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeatReservation.Domain.Events;

namespace SeatReservation.Infrastructure.Postgres.Configurations;

public class EventDetailsConfiguration : IEntityTypeConfiguration<EventDetails>
{
    public void Configure(EntityTypeBuilder<EventDetails> builder)
    {
        builder.ToTable("events_details");

        builder.HasKey(v => v.EventId).HasName("pk_event_details");

        builder.Property(v => v.EventId)
            .HasConversion(v => v.Value, id => new EventId(id))
            .HasColumnName("event_id");

        builder
            .HasOne<Event>()
            .WithOne(e => e.Details)
            .HasForeignKey<EventDetails>(ed => ed.EventId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}