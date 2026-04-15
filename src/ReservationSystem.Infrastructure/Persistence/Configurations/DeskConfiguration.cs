using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationSystem.Domain.Entities;

namespace ReservationSystem.Infrastructure.Persistence.Configurations;

internal sealed class DeskConfiguration : IEntityTypeConfiguration<Desk>
{
    public void Configure(EntityTypeBuilder<Desk> e)
    {
        e.ToTable("Desks");
        e.HasKey(d => d.Id);
        e.Property(d => d.Name).HasMaxLength(100).IsRequired();

        e.HasData(
            new Desk { Id = 1, Name = "Desk A1" },
            new Desk { Id = 2, Name = "Desk A2" },
            new Desk { Id = 3, Name = "Desk A3" },
            new Desk { Id = 4, Name = "Desk B1" },
            new Desk { Id = 5, Name = "Desk B2" });
    }
}
