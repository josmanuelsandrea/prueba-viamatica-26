using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViamaticaApi.Domain.Entities;

namespace ViamaticaApi.Infrastructure.Data.Configurations;

public class TurnConfiguration : IEntityTypeConfiguration<Turn>
{
    public void Configure(EntityTypeBuilder<Turn> builder)
    {
        builder.ToTable("turn");
        builder.HasKey(e => e.Turnid);
        builder.Property(e => e.Turnid).HasColumnName("turnid");
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(6).IsRequired();
        builder.Property(e => e.Date).HasColumnName("date");
        builder.Property(e => e.CashCashid).HasColumnName("cash_cashid");
        builder.Property(e => e.Usergestorid).HasColumnName("usergestorid");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");

        builder.HasOne(e => e.Cash)
            .WithMany(c => c.Turns)
            .HasForeignKey(e => e.CashCashid);

        builder.HasOne(e => e.Gestor)
            .WithMany()
            .HasForeignKey(e => e.Usergestorid)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
