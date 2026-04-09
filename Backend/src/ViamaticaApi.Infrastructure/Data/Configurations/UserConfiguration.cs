using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViamaticaApi.Domain.Entities;

namespace ViamaticaApi.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user");
        builder.HasKey(e => e.Userid);
        builder.Property(e => e.Userid).HasColumnName("userid");
        builder.Property(e => e.Username).HasColumnName("username").HasMaxLength(20).IsRequired();
        builder.Property(e => e.Email).HasColumnName("email").IsRequired();
        builder.Property(e => e.Password).HasColumnName("password").IsRequired();
        builder.Property(e => e.RolRolid).HasColumnName("rol_rolid");
        builder.Property(e => e.Creationdate).HasColumnName("creationdate");
        builder.Property(e => e.Usercreate).HasColumnName("usercreate");
        builder.Property(e => e.Userapproval).HasColumnName("userapproval");
        builder.Property(e => e.Dateapproval).HasColumnName("dateapproval");
        builder.Property(e => e.UserstatusStatusid).HasColumnName("userstatus_statusid").HasMaxLength(3);
        builder.Property(e => e.Active).HasColumnName("active");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(e => e.Username).IsUnique();

        builder.HasOne(e => e.Rol)
            .WithMany(r => r.Users)
            .HasForeignKey(e => e.RolRolid);

        builder.HasOne(e => e.UserStatus)
            .WithMany(s => s.Users)
            .HasForeignKey(e => e.UserstatusStatusid);

        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.Usercreate)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ApprovedByUser)
            .WithMany()
            .HasForeignKey(e => e.Userapproval)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
