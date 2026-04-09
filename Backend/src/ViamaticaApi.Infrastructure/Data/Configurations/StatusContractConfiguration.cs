using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViamaticaApi.Domain.Entities;

namespace ViamaticaApi.Infrastructure.Data.Configurations;

public class StatusContractConfiguration : IEntityTypeConfiguration<StatusContract>
{
    public void Configure(EntityTypeBuilder<StatusContract> builder)
    {
        builder.ToTable("statuscontract");
        builder.HasKey(e => e.Statusid);
        builder.Property(e => e.Statusid).HasColumnName("statusid").HasMaxLength(3);
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(50).IsRequired();
    }
}
