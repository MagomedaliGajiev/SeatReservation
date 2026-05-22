using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeatReservation.Domain;
using SharedKernel;

namespace SeatReservation.Infrastructure.Postgres.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id).HasName("pk_users");

        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.OwnsOne(u => u.Details, db =>
        {
            db.Property(u => u.Description)
                .IsRequired().HasMaxLength(LengthConstants.LENGTH500)
                .HasJsonPropertyName("description");

            db.Property(u => u.FIO)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH500)
                .HasJsonPropertyName("fio");

            db.ToJson("details");

            db.OwnsMany(d => d.Socials, sb =>
            {
                sb.Property(u => u.Link)
                    .IsRequired().HasMaxLength(LengthConstants.LENGTH500)
                    .HasJsonPropertyName("link");

                sb.Property(u => u.Name)
                    .IsRequired().HasMaxLength(LengthConstants.LENGTH500)
                    .HasJsonPropertyName("name");
            });
        });
    }
}