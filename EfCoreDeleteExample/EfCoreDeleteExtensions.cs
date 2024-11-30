using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EfCoreDeleteExample;

public static class EfCoreDeleteExtensions
{
    public static async Task<bool> DeleteByIdAsync<TEntity, TId>(
        this DbSet<TEntity> dbSet,
        TId id,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var context = dbSet.GetService<ICurrentDbContext>().Context;
        var entityType = context.Model.FindEntityType(typeof(TEntity));

        if (entityType == null)
            throw new InvalidOperationException($"Entity type '{typeof(TEntity).Name}' not found in the context.");

        var primaryKey = entityType.FindPrimaryKey();
        if (primaryKey == null)
            throw new InvalidOperationException($"No primary key defined for entity type '{typeof(TEntity).Name}'.");

        var keyProperty = primaryKey.Properties.FirstOrDefault();
        if (keyProperty == null)
            throw new InvalidOperationException($"No key property found for entity type '{typeof(TEntity).Name}'.");

        try
        {
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var property = Expression.Property(parameter, keyProperty.Name);
            var constant = Expression.Constant(id, typeof(TId));
            var equality = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(equality, parameter);

            var entity = await dbSet
                .Where(lambda)
                .Select(e => new { Id = EF.Property<TId>(e, keyProperty.Name) })
                .FirstOrDefaultAsync(cancellationToken);

            if (entity == null)
                return false;

            var deleteEntity = EntityFrameworkQueryableExtensions.State<TEntity>(context, keyProperty.Name, id!);
            context.Remove(deleteEntity);
            await context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> DeleteWhereAsync<TEntity>(
        this DbSet<TEntity> dbSet,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        try
        {
            var context = dbSet.GetService<ICurrentDbContext>().Context;
            var entityType = context.Model.FindEntityType(typeof(TEntity));

            if (entityType == null)
                throw new InvalidOperationException($"Entity type '{typeof(TEntity).Name}' not found in the context.");

            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey == null)
                throw new InvalidOperationException(
                    $"No primary key defined for entity type '{typeof(TEntity).Name}'.");

            var keyProperty = primaryKey.Properties.FirstOrDefault();
            if (keyProperty == null)
                throw new InvalidOperationException($"No key property found for entity type '{typeof(TEntity).Name}'.");

            var ids = await dbSet
                .Where(predicate)
                .Select(e => new { Id = EF.Property<object>(e, keyProperty.Name) })
                .ToListAsync(cancellationToken);

            if (!ids.Any())
                return false;

            foreach (var id in ids)
            {
                var deleteEntity = EntityFrameworkQueryableExtensions.State<TEntity>(context, keyProperty.Name, id.Id);
                context.Remove(deleteEntity);
            }

            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

internal static class EntityFrameworkQueryableExtensions
{
    public static TEntity State<TEntity>(DbContext context, string keyName, object keyValue) where TEntity : class
    {
        var entity = Activator.CreateInstance<TEntity>();
        var entry = context.Entry(entity);
        entry.Property(keyName).CurrentValue = keyValue;
        entry.State = EntityState.Deleted;
        return entity;
    }
}