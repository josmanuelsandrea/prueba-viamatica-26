using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViamaticaApi.Domain.Entities;

namespace ViamaticaApi.Infrastructure.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("client");
        builder.HasKey(e => e.Clientid);
        builder.Property(e => e.Clientid).HasColumnName("clientid");
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Lastname).HasColumnName("lastname").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Identification).HasColumnName("identification").HasMaxLength(13).IsRequired();
        builder.Property(e => e.Email).HasColumnName("email").HasMaxLength(120).IsRequired();
        builder.Property(e => e.Phonenumber).HasColumnName("phonenumber").HasMaxLength(13).IsRequired();
        builder.Property(e => e.Address).HasColumnName("address").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Referenceaddress).HasColumnName("referenceaddress").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Active).HasColumnName("active");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(e => e.Identification).IsUnique();
    }
}
