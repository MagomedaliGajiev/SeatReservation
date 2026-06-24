using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeatReservation.Domain.Events;
using SeatReservation.Domain.Reservations;

namespace SeatReservation.Infrastructure.Postgres.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");

        builder.HasKey(r => r.Id).HasName("pk_reservations");

        builder.Property(v => v.Id)
            .HasConversion(r => r.Value, id => new ReservationId(id))
            .HasColumnName("id");

        builder.Property(r => r.EventId)
            .HasConversion(e => e.Value, id => new EventId(id))
            .HasColumnName("event_id");

        builder.Property(r => r.UserId)
            .HasColumnName("user_id");

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasColumnName("status");

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at");

        builder.HasIndex(r => new { r.EventId, r.Status }).HasFilter("status IN ('Confirmed', 'Pending')");
    }
}