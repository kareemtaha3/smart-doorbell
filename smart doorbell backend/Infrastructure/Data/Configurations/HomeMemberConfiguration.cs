using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Infrastructure.Data.Configurations;

public class HomeMemberConfiguration : IEntityTypeConfiguration<HomeMember>
{
    public void Configure(EntityTypeBuilder<HomeMember> builder)
    {
        builder.ToTable("HomeMembers");

        builder.HasKey(hm => hm.Id);

        builder.Property(hm => hm.Role)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(hm => hm.CreatedAt)
            .IsRequired();

        builder.HasIndex(hm => new { hm.HomeId, hm.UserId })
            .IsUnique();

        builder.HasOne(hm => hm.Home)
            .WithMany(h => h.Members)
            .HasForeignKey(hm => hm.HomeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(hm => hm.User)
            .WithMany(u => u.HomeMemberships)
            .HasForeignKey(hm => hm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
