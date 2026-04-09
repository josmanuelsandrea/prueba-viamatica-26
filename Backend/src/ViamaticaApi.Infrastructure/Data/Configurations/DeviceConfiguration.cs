using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViamaticaApi.Domain.Entities;

namespace ViamaticaApi.Infrastructure.Data.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("device");
        builder.HasKey(e => e.Deviceid);
        builder.Property(e => e.Deviceid).HasColumnName("deviceid");
        builder.Property(e => e.Devicename).HasColumnName("devicename").HasMaxLength(50).IsRequired();
        builder.Property(e => e.ServiceServiceid).HasColumnName("service_serviceid");
        builder.Property(e => e.Active).HasColumnName("active");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(e => e.Service)
            .WithMany(s => s.Devices)
            .HasForeignKey(e => e.ServiceServiceid);
    }
}
