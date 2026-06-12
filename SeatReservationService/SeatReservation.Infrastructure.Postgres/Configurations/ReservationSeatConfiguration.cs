using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeatReservation.Domain.Events;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Infrastructure.Postgres.Configurations;

public class ReservationSeatConfiguration : IEntityTypeConfiguration<ReservationSeat>
{
    public void Configure(EntityTypeBuilder<ReservationSeat> builder)
    {
        builder.ToTable("reservation_seats");

        builder.HasKey(v => v.Id).HasName("pk_reservation_seats");

        builder.Property(v => v.Id)
            .HasConversion(v => v.Value, id => new ReservationSeatId(id))
            .HasColumnName("id");

        builder.Property(v => v.SeatId)
            .HasConversion(v => v.Value, id => new SeatId(id))
            .HasColumnName("seat_id")
            .IsRequired();

        builder.Property(v => v.ReservationId)
            .HasConversion(v => v.Value, id => new ReservationId(id))
            .HasColumnName("reservation_id");

        builder.Property(v => v.EventId)
            .HasConversion(v => v.Value, id => new EventId(id))
            .HasColumnName("event_id")
            .IsRequired();

        builder
            .HasOne(rs => rs.Reservation)
            .WithMany(r => r.ReservedSeats)
            .HasForeignKey(rs => rs.ReservationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Seat>()
            .WithMany()
            .HasForeignKey(rs => rs.SeatId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(rs => new
        {
            rs.EventId, rs.SeatId,
        }).IsUnique();
    }
}