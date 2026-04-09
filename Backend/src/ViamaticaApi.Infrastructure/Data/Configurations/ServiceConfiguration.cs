using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViamaticaApi.Domain.Entities;

namespace ViamaticaApi.Infrastructure.Data.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("service");
        builder.HasKey(e => e.Serviceid);
        builder.Property(e => e.Serviceid).HasColumnName("serviceid");
        builder.Property(e => e.Servicename).HasColumnName("servicename").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Servicedescription).HasColumnName("servicedescription").HasMaxLength(150).IsRequired();
        builder.Property(e => e.SpeedMbps).HasColumnName("speed_mbps");
        builder.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(10,2)");
        builder.Property(e => e.Active).HasColumnName("active");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");
    }
}
