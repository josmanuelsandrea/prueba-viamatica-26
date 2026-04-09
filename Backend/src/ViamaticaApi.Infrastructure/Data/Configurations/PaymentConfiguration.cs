using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViamaticaApi.Domain.Entities;

namespace ViamaticaApi.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(e => e.Paymentid);
        builder.Property(e => e.Paymentid).HasColumnName("paymentid");
        builder.Property(e => e.Paydate).HasColumnName("paydate");
        builder.Property(e => e.Amount).HasColumnName("amount").HasColumnType("decimal(10,2)");
        builder.Property(e => e.ClientClientid).HasColumnName("client_clientid");
        builder.Property(e => e.ContractContractid).HasColumnName("contract_contractid");
        builder.Property(e => e.MethodpaymentMethodpaymentid).HasColumnName("methodpayment_methodpaymentid");
        builder.Property(e => e.AttentionAttentionid).HasColumnName("attention_attentionid");
        builder.Property(e => e.Active).HasColumnName("active");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(e => e.Client)
            .WithMany(c => c.Payments)
            .HasForeignKey(e => e.ClientClientid);

        builder.HasOne(e => e.Contract)
            .WithMany(c => c.Payments)
            .HasForeignKey(e => e.ContractContractid);

        builder.HasOne(e => e.MethodPayment)
            .WithMany(mp => mp.Payments)
            .HasForeignKey(e => e.MethodpaymentMethodpaymentid);

        builder.HasOne(e => e.Attention)
            .WithMany(a => a.Payments)
            .HasForeignKey(e => e.AttentionAttentionid);
    }
}
