using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViamaticaApi.Domain.Entities;

namespace ViamaticaApi.Infrastructure.Data.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("rol");
        builder.HasKey(e => e.Rolid);
        builder.Property(e => e.Rolid).HasColumnName("rolid");
        builder.Property(e => e.Rolname).HasColumnName("rolname").HasMaxLength(50).IsRequired();
    }
}
