using System.Linq.Expressions;

namespace ERP.Core.Manager.Api.Domain.Commons.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> Entities { get; }
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct);
        Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken);
        Task<List<T>> ToListAsync(IQueryable<T> query, CancellationToken ct);
    }
}