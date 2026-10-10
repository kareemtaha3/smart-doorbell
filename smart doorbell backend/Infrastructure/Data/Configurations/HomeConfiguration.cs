using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Infrastructure.Data.Configurations;

public class HomeConfiguration : IEntityTypeConfiguration<Home>
{
    public void Configure(EntityTypeBuilder<Home> builder)
    {
        builder.ToTable("Homes");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(h => h.CreatedAt)
            .IsRequired();

        builder.HasOne(h => h.Owner)
            .WithMany(u => u.OwnedHomes)
            .HasForeignKey(h => h.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
