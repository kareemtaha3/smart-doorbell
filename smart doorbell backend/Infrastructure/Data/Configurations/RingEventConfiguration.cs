using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Infrastructure.Data.Configurations;

public class RingEventConfiguration : IEntityTypeConfiguration<RingEvent>
{
    public void Configure(EntityTypeBuilder<RingEvent> builder)
    {
        builder.ToTable("RingEvents");

        builder.HasKey(re => re.Id);

        builder.Property(re => re.DeviceEventId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(re => re.ReceivedAt)
            .IsRequired();

        builder.HasIndex(re => new { re.DeviceId, re.DeviceEventId })
            .IsUnique();

        builder.HasOne(re => re.Session)
            .WithMany(rs => rs.RingEvents)
            .HasForeignKey(re => re.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(re => re.Device)
            .WithMany(d => d.RingEvents)
            .HasForeignKey(re => re.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
