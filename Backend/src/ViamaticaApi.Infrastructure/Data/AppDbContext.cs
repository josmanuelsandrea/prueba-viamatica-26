using Microsoft.EntityFrameworkCore;
using ViamaticaApi.Domain.Entities;

namespace ViamaticaApi.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UserStatus> UserStatuses => Set<UserStatus>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Cash> Cashes => Set<Cash>();
    public DbSet<UserCash> UserCashes => Set<UserCash>();
    public DbSet<AttentionType> AttentionTypes => Set<AttentionType>();
    public DbSet<AttentionStatus> AttentionStatuses => Set<AttentionStatus>();
    public DbSet<Turn> Turns => Set<Turn>();
    public DbSet<Attention> Attentions => Set<Attention>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<StatusContract> StatusContracts => Set<StatusContract>();
    public DbSet<MethodPayment> MethodPayments => Set<MethodPayment>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global query filters for logical deletes
        modelBuilder.Entity<User>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Cash>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Client>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Service>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Device>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Contract>().HasQueryFilter(e => e.DeletedAt == null);
    }
}
