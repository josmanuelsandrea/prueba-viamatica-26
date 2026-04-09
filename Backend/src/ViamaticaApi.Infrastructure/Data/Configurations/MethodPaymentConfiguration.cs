using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViamaticaApi.Domain.Entities;

namespace ViamaticaApi.Infrastructure.Data.Configurations;

public class MethodPaymentConfiguration : IEntityTypeConfiguration<MethodPayment>
{
    public void Configure(EntityTypeBuilder<MethodPayment> builder)
    {
        builder.ToTable("methodpayment");
        builder.HasKey(e => e.Methodpaymentid);
        builder.Property(e => e.Methodpaymentid).HasColumnName("methodpaymentid");
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(50).IsRequired();
    }
}
