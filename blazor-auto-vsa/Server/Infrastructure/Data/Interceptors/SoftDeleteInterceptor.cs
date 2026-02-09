using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Server.Domain.Common;

namespace Server.Infrastructure.Data.Interceptors;

public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        DbContext? context = eventData.Context;
        if (context == null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry in context.ChangeTracker.Entries())
            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletableEntity softDeletableEntity)
            {
                softDeletableEntity.Enabled = false;
                entry.State = EntityState.Modified;
            }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}