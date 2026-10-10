using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Infrastructure.Data.Configurations;

public class CameraSnapshotConfiguration : IEntityTypeConfiguration<CameraSnapshot>
{
    public void Configure(EntityTypeBuilder<CameraSnapshot> builder)
    {
        builder.ToTable("CameraSnapshots");

        builder.HasKey(cs => cs.Id);

        builder.Property(cs => cs.Bucket)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(cs => cs.ObjectKey)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(cs => cs.SentAt)
            .IsRequired();

        builder.Property(cs => cs.AckStatus)
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasDefaultValue(AckStatus.Pending)
            .IsRequired();

        builder.Property(cs => cs.MediaUrl)
            .HasMaxLength(500);

        builder.Property(cs => cs.Resolution)
            .HasMaxLength(20);

        builder.HasOne(cs => cs.Session)
            .WithMany(rs => rs.CameraSnapshots)
            .HasForeignKey(cs => cs.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.Device)
            .WithMany(d => d.CameraSnapshots)
            .HasForeignKey(cs => cs.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
