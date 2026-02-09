using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Server.Domain;
using Server.Domain.Common;
using Server.Domain.Enums;
using Server.Infrastructure.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Server.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider;
    private List<AuditTrail> _temporaryAuditLogList = new();

    public AuditableEntityInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        DbContext? context = eventData.Context;
        if (context == null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

        _temporaryAuditLogList = GenerateAuditLogs(context);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        DbContext? context = eventData.Context;
        int saveResult = await base.SavedChangesAsync(eventData, result, cancellationToken);
        if (context != null) await FinalizeAuditLogsAsync(context, cancellationToken);
        return saveResult;
    }

    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await base.SaveChangesFailedAsync(eventData, cancellationToken);
        DbContext? context = eventData.Context;
        Exception exception = eventData.Exception;
        if (context != null)
        {
            string errorMessage =
                exception.InnerException != null ? exception.InnerException.Message : exception.Message;
            foreach (AuditTrail AuditLog in _temporaryAuditLogList) AuditLog.ErrorMessage = errorMessage;
            await SaveAuditLogsWithNewContextAsync(_temporaryAuditLogList, cancellationToken);
        }
    }

    private List<AuditTrail> GenerateAuditLogs(DbContext context)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        ICurrentUserService? currentUserService = scope.ServiceProvider.GetService<ICurrentUserService>();
        Guid? userId = currentUserService?.GetCurrentUserId();

        IDateTimeProvider? datetimeProvider = scope.ServiceProvider.GetService<IDateTimeProvider>();
        DateTime now = datetimeProvider?.GetUtcNow() ?? DateTime.UtcNow;

        List<AuditTrail> AuditLogs = new();

        foreach (EntityEntry entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is IAuditableEntity)
            {
                if (IsValidAuditEntry(entry))
                {
                    AuditTrail? AuditLog = CreateAuditLog(entry, userId, now);

                    if (AuditLog != null)
                        AuditLogs.Add(AuditLog);
                }
            }
        }

        return AuditLogs;
    }

    private static bool IsValidAuditEntry(EntityEntry entry) => entry.Entity is not AuditTrail &&
                                                                entry.State != EntityState.Detached &&
                                                                entry.State != EntityState.Unchanged;

    private static bool IsBinaryProperty(PropertyEntry property) => property.Metadata.ClrType == typeof(byte[]);

    private AuditTrail CreateAuditLog(EntityEntry entry, Guid? userId, DateTime now)
    {
        AuditTrail AuditLog = new()
        {
            TableName = entry.Metadata.GetTableName(),
            UserId = userId,
            DateTime = now,
            AffectedColumns = new List<string>(),
            NewValues = new Dictionary<string, object?>(),
            OldValues = new Dictionary<string, object?>(),
            AuditedEntity = (IAuditableEntity)entry.Entity
        };

        bool hasChanges = false;
        foreach (PropertyEntry property in entry.Properties)
        {
            if (property.IsTemporary)
            {
                AuditLog.TemporaryProperties.Add(new TemporaryProperty
                {
                    Name = property.Metadata.Name,
                    IsPrimaryKey = property.Metadata.IsPrimaryKey()
                });
                continue;
            }

            string propertyName = property.Metadata.Name;
            if (property.Metadata.IsPrimaryKey() && property.CurrentValue != null)
            {
                AuditLog.PrimaryKey[propertyName] = property.CurrentValue;
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    AuditLog.AuditType = AuditType.Created;
                    hasChanges = true;
                    if (property.CurrentValue != null)
                        AuditLog.NewValues[propertyName] =
                            IsBinaryProperty(property) ? "[Binary Data]" : property.CurrentValue;
                    break;

                case EntityState.Deleted:
                    AuditLog.AuditType = AuditType.Deleted;
                    hasChanges = true;
                    if (property.OriginalValue != null)
                        AuditLog.OldValues[propertyName] =
                            IsBinaryProperty(property) ? "[Binary Data]" : property.OriginalValue;
                    break;

                case EntityState.Modified
                    when property.IsModified && !Equals(property.OriginalValue, property.CurrentValue):
                    AuditLog.AuditType = AuditType.Updated;
                    hasChanges = true;
                    AuditLog.AffectedColumns.Add(propertyName);
                    if (property.OriginalValue != null)
                        AuditLog.OldValues[propertyName] =
                            IsBinaryProperty(property) ? "[Binary Data]" : property.OriginalValue;
                    if (property.CurrentValue != null)
                        AuditLog.NewValues[propertyName] =
                            IsBinaryProperty(property) ? "[Binary Data]" : property.CurrentValue;
                    break;
            }
        }

        // Skip creating audit log if no changes were detected
        if (!hasChanges) return null;

        return AuditLog;
    }

    private async Task FinalizeAuditLogsAsync(DbContext context, CancellationToken cancellationToken)
    {
        if (_temporaryAuditLogList.Any())
        {
            foreach (AuditTrail auditLog in
                     _temporaryAuditLogList) FinalizeTemporaryProperties(auditLog); // New: Extracted method for reuse

            await context.AddRangeAsync(_temporaryAuditLogList, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            _temporaryAuditLogList.Clear();
        }
    }

    private async Task SaveAuditLogsWithNewContextAsync(List<AuditTrail> AuditLogs, CancellationToken cancellationToken)
    {
        if (_temporaryAuditLogList.Any())
        {
            foreach (AuditTrail auditLog in
                     AuditLogs) FinalizeTemporaryProperties(auditLog); // New: Reuse the same finalization logic

            using IServiceScope scope = _serviceProvider.CreateScope();
            IDbContextFactory<ApplicationDbContext> dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            ApplicationDbContext dbcontext = dbContextFactory.CreateDbContext();
            await dbcontext.AddRangeAsync(AuditLogs, cancellationToken);
            await dbcontext.SaveChangesAsync(cancellationToken);

            _temporaryAuditLogList.Clear();
        }
    }

    private static void FinalizeTemporaryProperties(AuditTrail auditLog)
    {
        if (auditLog.AuditedEntity == null) return;

        foreach (TemporaryProperty temp in auditLog.TemporaryProperties)
        {
            System.Reflection.PropertyInfo? propertyInfo = auditLog.AuditedEntity.GetType().GetProperty(temp.Name);
            if (propertyInfo == null) continue; // Skip if not found (edge case)

            object? currentValue = propertyInfo.GetValue(auditLog.AuditedEntity);
            object? value = propertyInfo.PropertyType == typeof(byte[]) ? "[Binary Data]" : currentValue;

            if (temp.IsPrimaryKey && value != null)
                auditLog.PrimaryKey[temp.Name] = value;
            else if (auditLog.NewValues != null && value != null) auditLog.NewValues[temp.Name] = value;
        }

        // Optional: Clear temporary properties after finalization to free memory
        auditLog.TemporaryProperties.Clear();
    }
}