using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Infrastructure.Data.Configurations;

public class HomeInvitationConfiguration : IEntityTypeConfiguration<HomeInvitation>
{
    public void Configure(EntityTypeBuilder<HomeInvitation> builder)
    {
        builder.ToTable("HomeInvitations");

        builder.HasKey(hi => hi.Id);

        builder.Property(hi => hi.Status)
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasDefaultValue(InvitationStatus.Pending)
            .IsRequired();

        builder.Property(hi => hi.CreatedAt)
            .IsRequired();

        builder.HasIndex(hi => new { hi.HomeId, hi.InvitedUserId })
            .IsUnique();

        builder.HasOne(hi => hi.Home)
            .WithMany(h => h.Invitations)
            .HasForeignKey(hi => hi.HomeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(hi => hi.InvitedByUser)
            .WithMany(u => u.SentInvitations)
            .HasForeignKey(hi => hi.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(hi => hi.InvitedUser)
            .WithMany(u => u.ReceivedInvitations)
            .HasForeignKey(hi => hi.InvitedUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
