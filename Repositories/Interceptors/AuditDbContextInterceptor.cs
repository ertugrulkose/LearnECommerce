using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace App.Repositories.Interceptors
{
    public class AuditDbContextInterceptor : SaveChangesInterceptor
    {
        private static readonly Action<DbContext, IAuditEntity> AddBehavior = (dbContext, auditEntity) =>
        {
            auditEntity.Created = DateTime.UtcNow;
            dbContext.Entry(auditEntity).Property(x => x.Updated).IsModified = false;
        };

        private static readonly Action<DbContext, IAuditEntity> ModifiedBehavior = (dbContext, auditEntity) =>
        {
            auditEntity.Updated = DateTime.UtcNow;
            dbContext.Entry(auditEntity).Property(x => x.Created).IsModified = false;
        };

        private static readonly Dictionary<EntityState, Action<DbContext, IAuditEntity>> Behaviors = new()
            {
                { EntityState.Added, AddBehavior },
                { EntityState.Modified, ModifiedBehavior }
            };

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context!;
            foreach (var entityEntry in context.ChangeTracker.Entries<IAuditEntity>())
            {
                if (Behaviors.TryGetValue(entityEntry.State, out var behavior))
                {
                    behavior(context, entityEntry.Entity);
                }
            }
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}

    //private static readonly Dictionary<EntityState, Action<DbContext, IAuditEntity>> Behaviors = new()
    //{
    //    {EntityState.Added, AddBehavior},
    //    {EntityState.Modified, ModifiedBehavior}
    //};

    //public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
    //    CancellationToken cancellationToken = new CancellationToken())
    //{
    //    foreach (var entityEntry in eventData.Context!.ChangeTracker.Entries().ToList())
    //    {
    //        if (entityEntry.Entity is not IAuditEntity auditEntity) continue;

    //        if (entityEntry.State is not (EntityState.Added or EntityState.Modified)) continue;

    //        Behaviors[entityEntry.State](eventData.Context, auditEntity);

    //        #region 1.Way (Switch Case)
    //        //switch (entityEntry.State)
    //        //{
    //        //    case EntityState.Added:

    //        //        AddBehavior(eventData.Context, auditEntity);

    //        //    break;

    //        //    case EntityState.Modified:

    //        //        ModifiedBehavior(eventData.Context, auditEntity);

    //        //    break;
    //        //}
    //        #endregion
    //    }
    //    return base.SavingChangesAsync(eventData, result, cancellationToken);
    //}

    //private static void AddBehavior(DbContext dbContext,IAuditEntity auditEntity)
    //{
    //    auditEntity.Created = DateTime.UtcNow;
    //    dbContext.Entry(auditEntity).Property(x => x.Updated).IsModified = false;
    //}

    //private static void ModifiedBehavior(DbContext dbContext, IAuditEntity auditEntity)
    //{
    //    auditEntity.Updated = DateTime.UtcNow;
    //    dbContext.Entry(auditEntity).Property(x => x.Created).IsModified = false;
    //}
