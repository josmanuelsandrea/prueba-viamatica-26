using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViamaticaApi.Domain.Entities;

namespace ViamaticaApi.Infrastructure.Data.Configurations;

public class AttentionStatusConfiguration : IEntityTypeConfiguration<AttentionStatus>
{
    public void Configure(EntityTypeBuilder<AttentionStatus> builder)
    {
        builder.ToTable("attentionstatus");
        builder.HasKey(e => e.Statusid);
        builder.Property(e => e.Statusid).HasColumnName("statusid");
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(30).IsRequired();
    }
}
