using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.Domain.Entities;
using Shared.Domain.Enums;
using Server.Infrastructure.Data.Configurations;
using Server.Infrastructure.Data.Converters;

namespace Server.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AuditTrail> AuditTrails { get; set; } = null!;
    
    public DbSet<Product> Products { get; set; } = null!;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>()
            .HaveColumnType("timestamp with time zone");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.HasPostgresEnum<AuditType>();

        base.OnModelCreating(modelBuilder);
    }
}
