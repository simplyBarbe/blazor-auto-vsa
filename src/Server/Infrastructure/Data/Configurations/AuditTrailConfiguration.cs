using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Domain;
using Server.Extensions;

namespace Server.Infrastructure.Data.Configurations;


public class AuditTrailConfiguration : IEntityTypeConfiguration<AuditTrail>
{
    public void Configure(EntityTypeBuilder<AuditTrail> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(x => x.Id);


        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TableName).HasColumnName("table_name");
        builder.Property(x => x.DateTime).HasColumnName("datetime");
        builder.Property(x => x.UserId).HasColumnName("id_utente");
        builder.Property(t => t.AuditType).HasColumnName("type").HasConversion<int>();
        builder.Property(e => e.AffectedColumns).HasColumnName("affected_columns").HasJsonConversion();
        builder.Property(u => u.OldValues).HasColumnName("old_values").HasJsonConversion();
        builder.Property(u => u.NewValues).HasColumnName("new_values").HasJsonConversion();
        builder.Property(u => u.PrimaryKey).HasColumnName("keys").HasJsonConversion();
        builder.Ignore(x => x.TemporaryProperties);
        builder.Ignore(x => x.AuditedEntity);
        builder.Property(x => x.ErrorMessage).HasColumnName("error_message").HasMaxLength(int.MaxValue);
    }
}