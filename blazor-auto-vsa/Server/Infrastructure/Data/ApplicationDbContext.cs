using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Shared.Domain.Enums;
using Server.Infrastructure.Data.Configurations;
using Server.Infrastructure.Data.Converters;

namespace Server.Infrastructure.Data;

/// <summary>
/// Application database context for Entity Framework Core.
/// Manages entity configurations and database connections.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AuditTrail> AuditTrails { get; set; } = null!;
    
    public DbSet<Product> Products { get; set; } = null!;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        // Apply UTC converter to all DateTime properties globally
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>()
            .HaveColumnType("timestamp with time zone");

    /// <summary>
    /// Configures the model using entity configurations from the Shared assembly.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.HasPostgresEnum<AuditType>();

        base.OnModelCreating(modelBuilder);
    }
}
