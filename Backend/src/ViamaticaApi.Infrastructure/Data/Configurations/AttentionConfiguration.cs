using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViamaticaApi.Domain.Entities;

namespace ViamaticaApi.Infrastructure.Data.Configurations;

public class AttentionConfiguration : IEntityTypeConfiguration<Attention>
{
    public void Configure(EntityTypeBuilder<Attention> builder)
    {
        builder.ToTable("attention");
        builder.HasKey(e => e.Attentionid);
        builder.Property(e => e.Attentionid).HasColumnName("attentionid");
        builder.Property(e => e.TurnTurnid).HasColumnName("turn_turnid");
        builder.Property(e => e.ClientClientid).HasColumnName("client_clientid");
        builder.Property(e => e.AttentiontypeAttentiontypeid).HasColumnName("attentiontype_attentiontypeid").HasMaxLength(3);
        builder.Property(e => e.AttentionstatusStatusid).HasColumnName("attentionstatus_statusid");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(e => e.Turn)
            .WithMany(t => t.Attentions)
            .HasForeignKey(e => e.TurnTurnid);

        builder.HasOne(e => e.Client)
            .WithMany(c => c.Attentions)
            .HasForeignKey(e => e.ClientClientid);

        builder.HasOne(e => e.AttentionType)
            .WithMany(at => at.Attentions)
            .HasForeignKey(e => e.AttentiontypeAttentiontypeid);

        builder.HasOne(e => e.AttentionStatus)
            .WithMany(as_ => as_.Attentions)
            .HasForeignKey(e => e.AttentionstatusStatusid);
    }
}
