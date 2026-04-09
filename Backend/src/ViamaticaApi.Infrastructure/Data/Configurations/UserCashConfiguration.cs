using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViamaticaApi.Domain.Entities;

namespace ViamaticaApi.Infrastructure.Data.Configurations;

public class UserCashConfiguration : IEntityTypeConfiguration<UserCash>
{
    public void Configure(EntityTypeBuilder<UserCash> builder)
    {
        builder.ToTable("usercash");
        builder.HasKey(e => new { e.UserUserid, e.CashCashid });
        builder.Property(e => e.UserUserid).HasColumnName("user_userid");
        builder.Property(e => e.CashCashid).HasColumnName("cash_cashid");
        builder.Property(e => e.IsActive).HasColumnName("is_active");
        builder.Property(e => e.AssignedAt).HasColumnName("assigned_at");

        builder.HasOne(e => e.User)
            .WithMany(u => u.UserCashes)
            .HasForeignKey(e => e.UserUserid);

        builder.HasOne(e => e.Cash)
            .WithMany(c => c.UserCashes)
            .HasForeignKey(e => e.CashCashid);
    }
}
