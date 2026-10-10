using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Infrastructure.Data.Configurations;

public class RingSessionConfiguration : IEntityTypeConfiguration<RingSession>
{
    public void Configure(EntityTypeBuilder<RingSession> builder)
    {
        builder.ToTable("RingSessions");

        builder.HasKey(rs => rs.Id);

        builder.Property(rs => rs.CreatedAt)
            .IsRequired();

        builder.Property(rs => rs.LastPressAt)
            .IsRequired();

        builder.Property(rs => rs.PressCount)
            .HasDefaultValue(1)
            .IsRequired();

        builder.HasOne(rs => rs.ButtonDevice)
            .WithMany(d => d.RingSessions)
            .HasForeignKey(rs => rs.ButtonDeviceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rs => rs.Home)
            .WithMany(h => h.RingSessions)
            .HasForeignKey(rs => rs.HomeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
