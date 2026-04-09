using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViamaticaApi.Domain.Entities;

namespace ViamaticaApi.Infrastructure.Data.Configurations;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("contract");
        builder.HasKey(e => e.Contractid);
        builder.Property(e => e.Contractid).HasColumnName("contractid");
        builder.Property(e => e.Startdate).HasColumnName("startdate");
        builder.Property(e => e.Enddate).HasColumnName("enddate");
        builder.Property(e => e.ServiceServiceid).HasColumnName("service_serviceid");
        builder.Property(e => e.StatuscontractStatusid).HasColumnName("statuscontract_statusid").HasMaxLength(3);
        builder.Property(e => e.ClientClientid).HasColumnName("client_clientid");
        builder.Property(e => e.MethodpaymentMethodpaymentid).HasColumnName("methodpayment_methodpaymentid");
        builder.Property(e => e.AttentionAttentionid).HasColumnName("attention_attentionid");
        builder.Property(e => e.ParentContractid).HasColumnName("parent_contractid");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(e => e.Service)
            .WithMany(s => s.Contracts)
            .HasForeignKey(e => e.ServiceServiceid);

        builder.HasOne(e => e.StatusContract)
            .WithMany(sc => sc.Contracts)
            .HasForeignKey(e => e.StatuscontractStatusid);

        builder.HasOne(e => e.Client)
            .WithMany(c => c.Contracts)
            .HasForeignKey(e => e.ClientClientid);

        builder.HasOne(e => e.MethodPayment)
            .WithMany(mp => mp.Contracts)
            .HasForeignKey(e => e.MethodpaymentMethodpaymentid);

        builder.HasOne(e => e.Attention)
            .WithMany(a => a.Contracts)
            .HasForeignKey(e => e.AttentionAttentionid);

        builder.HasOne(e => e.ParentContract)
            .WithMany(c => c.ChildContracts)
            .HasForeignKey(e => e.ParentContractid)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
