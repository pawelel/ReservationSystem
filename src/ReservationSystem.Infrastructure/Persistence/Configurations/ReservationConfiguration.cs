using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationSystem.Domain.Entities;

namespace ReservationSystem.Infrastructure.Persistence.Configurations;

internal sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> e)
    {
        e.ToTable("Reservations", t =>
        {
            t.HasCheckConstraint("CHK_Reservations_Dates", "[EndAt] > [StartAt]");
            t.HasCheckConstraint("CHK_Reservations_Status", "[Status] IN (0, 1)");
        });

        e.HasKey(r => r.Id);

        e.Property(r => r.Status).HasConversion<int>().IsRequired();
        e.Property(r => r.StartAt).HasColumnType("datetime2(0)");
        e.Property(r => r.EndAt).HasColumnType("datetime2(0)");
        e.Property(r => r.CreatedAt)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        e.HasOne(r => r.Desk)
            .WithMany()
            .HasForeignKey(r => r.DeskId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasIndex(r => new { r.DeskId, r.Status })
            .HasDatabaseName("IX_Reservations_DeskId_Status")
            .IncludeProperties(r => new { r.StartAt, r.EndAt });
    }
}
