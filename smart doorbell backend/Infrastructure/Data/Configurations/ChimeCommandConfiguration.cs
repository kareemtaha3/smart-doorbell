using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Infrastructure.Data.Configurations;

public class ChimeCommandConfiguration : IEntityTypeConfiguration<ChimeCommand>
{
    public void Configure(EntityTypeBuilder<ChimeCommand> builder)
    {
        builder.ToTable("ChimeCommands");

        builder.HasKey(cc => cc.Id);

        builder.Property(cc => cc.SentAt)
            .IsRequired();

        builder.Property(cc => cc.AckStatus)
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasDefaultValue(AckStatus.Pending)
            .IsRequired();

        builder.HasOne(cc => cc.RingEvent)
            .WithMany(re => re.ChimeCommands)
            .HasForeignKey(cc => cc.RingEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cc => cc.Device)
            .WithMany(d => d.ChimeCommands)
            .HasForeignKey(cc => cc.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cc => cc.Melody)
            .WithMany(m => m.ChimeCommands)
            .HasForeignKey(cc => cc.MelodyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
