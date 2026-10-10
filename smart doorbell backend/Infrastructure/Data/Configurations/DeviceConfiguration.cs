using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Infrastructure.Data.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Type)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(d => d.AuthToken)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(d => d.AuthToken)
            .IsUnique();

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasDefaultValue(DeviceStatus.Offline)
            .IsRequired();

        builder.HasIndex(d => new { d.HomeId, d.Type })
            .IsUnique();

        builder.HasOne(d => d.Home)
            .WithMany(h => h.Devices)
            .HasForeignKey(d => d.HomeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
