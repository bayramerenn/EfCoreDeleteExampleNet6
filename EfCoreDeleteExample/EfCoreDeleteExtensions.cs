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

        var keyProperty = entityType.FindPrimaryKey().Properties.First();

        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var property = Expression.Property(parameter, keyProperty.Name);
        var constant = Expression.Constant(id, typeof(TId));
        var equality = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<TEntity, bool>>(equality, parameter);

        var query = dbSet.Where(lambda);

        try
        {
            dbSet.RemoveRange(await query.ToListAsync(cancellationToken));
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
            var entities = await dbSet.Where(predicate).ToListAsync(cancellationToken);
            dbSet.RemoveRange(entities);
            return true;
        }
        catch
        {
            return false;
        }
    }
}