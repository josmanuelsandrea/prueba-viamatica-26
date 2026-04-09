using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViamaticaApi.Domain.Entities;

namespace ViamaticaApi.Infrastructure.Data.Configurations;

public class AttentionTypeConfiguration : IEntityTypeConfiguration<AttentionType>
{
    public void Configure(EntityTypeBuilder<AttentionType> builder)
    {
        builder.ToTable("attentiontype");
        builder.HasKey(e => e.Attentiontypeid);
        builder.Property(e => e.Attentiontypeid).HasColumnName("attentiontypeid").HasMaxLength(3);
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Prefix).HasColumnName("prefix").HasMaxLength(2).IsRequired();
        builder.HasIndex(e => e.Prefix).IsUnique();
    }
}
