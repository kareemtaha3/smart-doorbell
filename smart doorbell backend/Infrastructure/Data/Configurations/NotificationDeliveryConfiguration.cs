using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Infrastructure.Data.Configurations;

public class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
    {
        builder.ToTable("NotificationDeliveries");

        builder.HasKey(nd => nd.Id);

        builder.Property(nd => nd.Channel)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(nd => nd.SentAt)
            .IsRequired();

        builder.Property(nd => nd.Status)
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasDefaultValue(DeliveryStatus.Sent)
            .IsRequired();

        builder.HasIndex(nd => new { nd.NotificationId, nd.UserId, nd.Channel })
            .IsUnique();

        builder.HasOne(nd => nd.Notification)
            .WithMany(n => n.Deliveries)
            .HasForeignKey(nd => nd.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(nd => nd.User)
            .WithMany(u => u.NotificationDeliveries)
            .HasForeignKey(nd => nd.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
