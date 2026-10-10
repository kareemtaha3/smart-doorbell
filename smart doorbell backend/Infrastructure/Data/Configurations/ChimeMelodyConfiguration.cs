using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Infrastructure.Data.Configurations;

public class ChimeMelodyConfiguration : IEntityTypeConfiguration<ChimeMelody>
{
    public void Configure(EntityTypeBuilder<ChimeMelody> builder)
    {
        builder.ToTable("ChimeMelodies");

        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(cm => cm.ToneFileUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(cm => cm.IsMain)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(cm => cm.CreatedAt)
            .IsRequired();

        builder.HasIndex(cm => cm.HomeId)
            .HasDatabaseName("UX_ChimeMelodies_OneMainPerHome")
            .IsUnique()
            .HasFilter("[IsMain] = 1");

        builder.HasOne(cm => cm.Home)
            .WithMany(h => h.ChimeMelodies)
            .HasForeignKey(cm => cm.HomeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
