using Microsoft.EntityFrameworkCore;

namespace FM.Infrastructure.EfCore
{
    /// <summary>
    /// Dummy DbSet to avoid nullable warning
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class NullDbSet<TEntity> : DbSet<TEntity> where TEntity : class
    {
    }
}
