using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(n => n.Body)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(n => n.PressCount)
            .IsRequired();

        builder.Property(n => n.ImageUrl)
            .HasMaxLength(500);

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.Property(n => n.UpdatedAt)
            .IsRequired();

        builder.HasIndex(n => n.SessionId)
            .IsUnique();

        builder.HasOne(n => n.Session)
            .WithOne(rs => rs.Notification)
            .HasForeignKey<Notification>(n => n.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
