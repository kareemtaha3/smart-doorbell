using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Infrastructure.Data.Configurations;

public class PushSubscriptionConfiguration : IEntityTypeConfiguration<PushSubscription>
{
    public void Configure(EntityTypeBuilder<PushSubscription> builder)
    {
        builder.ToTable("PushSubscriptions");

        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.Endpoint)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(ps => ps.P256dhKey)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(ps => ps.AuthKey)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(ps => ps.CreatedAt)
            .IsRequired();

        builder.HasIndex(ps => new { ps.UserId, ps.Endpoint })
            .IsUnique();

        builder.HasOne(ps => ps.User)
            .WithMany(u => u.PushSubscriptions)
            .HasForeignKey(ps => ps.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
